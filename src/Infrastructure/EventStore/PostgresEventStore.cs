using System.Text.Json;
using Dapper;
using Med.Labs.Domain.Interfaces;
using Npgsql;

namespace Med.Labs.Infrastructure.EventStore;

public class PostgresEventStore
{
	private readonly string _connectionString;
	private readonly IServiceProvider _services;

	public PostgresEventStore(string connectionString, IServiceProvider services)
	{
		_connectionString = connectionString;
		_services = services;
	}

	public async Task AppendEvent(Guid aggregateId, IDomainEvent @event)
	{
		var type = @event.GetType().FullName!;
		// Use the runtime type so System.Text.Json serializes the concrete event properties,
		// otherwise the generic overload infers IDomainEvent and you get "{}".
		var data = JsonSerializer.Serialize(@event, @event.GetType());
		var id = Guid.NewGuid();
		var version = DateTime.UtcNow.Ticks;

		await using var conn = new NpgsqlConnection(_connectionString);
		await conn.ExecuteAsync(
			"INSERT INTO event_store (id, aggregate_id, event_type, event_data, created_at, version) " +
			"VALUES (@Id, @AggregateId, @Type, @Data::jsonb, NOW(), @Version)",
			new { Id = id, AggregateId = aggregateId, Type = type, Data = data, Version = version });
	}

	public async Task<IEnumerable<IDomainEvent>> LoadEvents(Guid aggregateId, int fromVersion = 0)
	{
		await using var conn = new NpgsqlConnection(_connectionString);
		var rows = await conn.QueryAsync("SELECT event_type, event_data FROM event_store WHERE aggregate_id = @AggregateId ORDER BY created_at ASC",
			new { AggregateId = aggregateId });

		var events = new List<IDomainEvent>();
		foreach (var row in rows)
		{
			var typeName = (string)row.event_type;
			// Try to resolve the type by the stored name. First try Type.GetType (works for assembly-qualified names),
			// then search loaded assemblies for a matching full name.
			var type = Type.GetType(typeName)
					   ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).FirstOrDefault(t => t != null);

			if (type is null)
			{
				// Fail fast with a clear message instead of a null returnType which causes ArgumentNullException inside JsonSerializer.
				throw new InvalidOperationException($"Unable to resolve event CLR type '{typeName}'. Ensure event_type was stored as an assembly-qualified name or the assembly is loaded.");
			}

			// event_data can be returned as a JSON token/object; use ToString() to get JSON text
			var data = row.event_data?.ToString() ?? string.Empty;
			var obj = JsonSerializer.Deserialize(data, type!) as IDomainEvent;
			if (obj != null) events.Add(obj);
		}

		return events;
	}
}
