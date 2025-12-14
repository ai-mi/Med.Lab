using System.Text;
using Dapper;
using Npgsql;

namespace Med.Labs.Infrastructure.Outbox;

public class OutboxPublisher
{
	private readonly string _connectionString;
	private readonly int _batchSize = 25;
	private readonly TimeSpan _baseBackoff = TimeSpan.FromSeconds(5);

	public OutboxPublisher(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task PublishPending(CancellationToken ct = default)
	{
		// Instance id helps claim tracing across processes
		var instanceId = Guid.NewGuid();

		await using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync(ct);

		// Claim a batch (requires schema with locked_by, locked_at, next_attempt_at)
		await using var tx = await conn.BeginTransactionAsync(ct);
		var claimed = (await conn.QueryAsync(
			@"
WITH to_claim AS (
  SELECT id
  FROM outbox
  WHERE published = false
    AND (next_attempt_at IS NULL OR next_attempt_at <= NOW())
  ORDER BY created_at
  FOR UPDATE SKIP LOCKED
  LIMIT @Batch
)
UPDATE outbox
SET locked_by = @InstanceId, locked_at = NOW()
WHERE id IN (SELECT id FROM to_claim)
RETURNING id, payload, event_type
",
			new { Batch = _batchSize, InstanceId = instanceId },
			transaction: tx)).ToList();

		await tx.CommitAsync(ct);

		if (!claimed.Any())
			return;

		foreach (var row in claimed)
		{
			if (ct.IsCancellationRequested) break;

			var id = (Guid)row.id;
			var payload = row.payload?.ToString() ?? "{}";
			var eventType = (string)row.event_type;

			try
			{
				// Replace with real transport publish (Kafka, RabbitMQ, etc.)
				await PublishToTransportAsync(eventType, payload, ct);

				// mark published and clear lock
				await conn.ExecuteAsync(
					@"UPDATE outbox SET published = true, published_at = NOW(), locked_by = NULL, locked_at = NULL WHERE id = @Id",
					new { Id = id });
			}
			catch (Exception ex)
			{
				// On failure increment attempts and set next_attempt_at (exponential backoff)
				// Uses attempts column; if not present, consider simple logging and leaving published=false
				await conn.ExecuteAsync(
					@"
UPDATE outbox
SET attempts = COALESCE(attempts,0) + 1,
    last_error = @Error,
    next_attempt_at = NOW() + (@BackoffSeconds || ' seconds')::interval,
    locked_by = NULL,
    locked_at = NULL
WHERE id = @Id",
					new
					{
						Id = id,
						Error = ex.ToString(),
						BackoffSeconds = Math.Min((int)Math.Pow(2, (await GetAttempts(conn, id)) ?? 0) * (int)_baseBackoff.TotalSeconds, 3600)
					});
			}
		}
	}

	private static async Task<int?> GetAttempts(NpgsqlConnection conn, Guid id)
	{
		var attempts = await conn.QueryFirstOrDefaultAsync<int?>("SELECT attempts FROM outbox WHERE id = @Id", new { Id = id });
		return attempts;
	}

	private Task PublishToTransportAsync(string eventType, string payload, CancellationToken ct)
	{
		// TODO: integrate with real message broker; must be idempotent on the consumer side.
		Console.WriteLine($"Publishing {eventType}: {payload}");
		return Task.CompletedTask;
	}
}
