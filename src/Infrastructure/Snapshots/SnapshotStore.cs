using Dapper;
using Med.Labs.Domain.Snapshots;
using Npgsql;
using System.Text.Json;

namespace Med.Labs.Infrastructure.Snapshots;

public class SnapshotStore
{
	private readonly string _connectionString;

	public SnapshotStore(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task SaveSnapshot(LaboratorySnapshot snapshot)
	{
		var json = JsonSerializer.Serialize(snapshot);
		await using var conn = new NpgsqlConnection(_connectionString);
		await conn.ExecuteAsync(
			@"INSERT INTO snapshot_store (patient_id, snapshot_data, version, created_at)
              VALUES (@PatientId, @Data::jsonb, @Version, NOW())
              ON CONFLICT (patient_id) DO UPDATE 
              SET snapshot_data = @Data::jsonb, version = @Version, created_at = NOW()",
			new { snapshot.PatientId, Data = json, snapshot.Version });
	}

	public async Task<LaboratorySnapshot?> LoadSnapshot(Guid patientId)
	{
		await using var conn = new NpgsqlConnection(_connectionString);
		var row = await conn.QueryFirstOrDefaultAsync<string>(
			"SELECT snapshot_data::text FROM snapshot_store WHERE patient_id = @PatientId ORDER BY created_at DESC LIMIT 1",
			new { PatientId = patientId });

		if (row == null) return null;
		return JsonSerializer.Deserialize<LaboratorySnapshot>(row);
	}
}
