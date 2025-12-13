using System.Linq;
using System.Threading.Tasks;
using Med.Labs.Domain.Aggregates;

namespace Med.Labs.Application.Queries;

public class LaboratoryQueryService
{
	private readonly IQueryRunner _runner;

	public LaboratoryQueryService(IQueryRunner runner)
	{
		_runner = runner;
	}


	public async Task<IEnumerable<LaboratoryResult>> GetAll(Guid patientId)
	{
		var sql = @"
SELECT
  result_id   AS ResultId,
  patient_id  AS PatientId,
  test_type   AS TestType,
  result      AS Result,
  normal_min  AS NormalMin,
  normal_max  AS NormalMax,
  comment     AS Comment,
  created_at  AS CreatedAt
FROM laboratory_read_model
WHERE patient_id = @PatientId";

		var rows = await _runner.QueryAsync<LaboratoryReadModel>(sql, new { PatientId = patientId });
		return rows.Select(MapToDomain);
	}

	public async Task<IEnumerable<LaboratoryResult>> GetByTestType(Guid patientId, string testType)
	{
		var sql = @"
SELECT
  result_id   AS ResultId,
  patient_id  AS PatientId,
  test_type   AS TestType,
  result      AS Result,
  normal_min  AS NormalMin,
  normal_max  AS NormalMax,
  comment     AS Comment,
  created_at  AS CreatedAt
FROM laboratory_read_model
WHERE patient_id = @PatientId AND test_type = @TestType";

		var rows = await _runner.QueryAsync<LaboratoryReadModel>(sql, new { PatientId = patientId, TestType = testType });
		return rows.Select(MapToDomain);
	}

	public async Task<IEnumerable<LaboratoryResult>> GetByDateRange(Guid patientId, DateTime from, DateTime to)
	{
		var sql = @"
SELECT
  result_id   AS ResultId,
  patient_id  AS PatientId,
  test_type   AS TestType,
  result      AS Result,
  normal_min  AS NormalMin,
  normal_max  AS NormalMax,
  comment     AS Comment,
  created_at  AS CreatedAt
FROM laboratory_read_model
WHERE patient_id = @PatientId AND created_at BETWEEN @From AND @To";

		var rows = await _runner.QueryAsync<LaboratoryReadModel>(sql, new { PatientId = patientId, From = from, To = to });
		return rows.Select(MapToDomain);
	}

	private static LaboratoryResult MapToDomain(LaboratoryReadModel r)
	{
		return new LaboratoryResult(r.ResultId, r.TestType ?? string.Empty, r.Result, r.NormalMin, r.NormalMax, r.Comment);
	}
}
