using Dapper;
using Npgsql;

namespace Med.Labs.Infrastructure.Outbox;

public class OutboxPublisher
{
	private readonly string _connectionString;

	public OutboxPublisher(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task PublishPending()
	{
		await using var conn = new NpgsqlConnection(_connectionString);
		var events = await conn.QueryAsync("SELECT id, payload, event_type FROM outbox WHERE published = false");

		foreach (var evt in events)
		{
			// Her kan evt. integreres med Kafka, RabbitMQ eller annet system
			Console.WriteLine($"Publishing {evt.event_type} {evt.id}");

			await conn.ExecuteAsync("UPDATE outbox SET published=true WHERE id=@Id", new { Id = evt.id });
		}
	}
}
