using Dapper;
using Med.Labs.Domain.Events;
using Npgsql;

namespace Med.Labs.Infrastructure.Projections;

public class LaboratoryProjectionHandler
{
	private readonly string _connectionString;

	public LaboratoryProjectionHandler(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task Project(object @event)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		switch (@event)
		{
			case LaboratoryResultAdded_v3 e:
				await conn.ExecuteAsync(
					@"INSERT INTO laboratory_read_model (result_id, patient_id, test_type, result, normal_min, normal_max, comment, created_at)
                      VALUES (@ResultId, @PatientId, @TestType, @Result, @NormalMin, @NormalMax, @Comment, NOW())",
					e);
				break;

			case LaboratoryResultUpdated_v2 e:
				await conn.ExecuteAsync(
					@"UPDATE laboratory_read_model SET test_type=@TestType, result=@Result, normal_min=@NormalMin, normal_max=@NormalMax, comment=@Comment
                      WHERE result_id=@ResultId AND patient_id=@PatientId",
					e);
				break;

			case LaboratoryResultDeleted_v1 e:
				await conn.ExecuteAsync(
					"DELETE FROM laboratory_read_model WHERE result_id=@ResultId AND patient_id=@PatientId",
					e);
				break;
		}
	}
}
