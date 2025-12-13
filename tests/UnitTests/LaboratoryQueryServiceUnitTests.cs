using FluentAssertions;
using Xunit;
using Med.Labs.Application.Queries;

namespace UnitTests;

public class LaboratoryQueryServiceUnitTests
{
	private class FakeQueryRunner : IQueryRunner
	{
		private readonly IEnumerable<object> _rows;

		public FakeQueryRunner(IEnumerable<object> rows) => _rows = rows;

		public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
			=> Task.FromResult(_rows.Cast<T>());
	}

	[Fact]
	public async Task GetAll_MapsRowsToDomain()
	{
		// arrange
		var patientId = Guid.NewGuid();
		var readRows = new[]
		{
			new LaboratoryReadModel
			{
				ResultId = Guid.NewGuid(),
				PatientId = patientId,
				TestType = "Hemoglobin",
				Result = 13.2,
				NormalMin = 12.0,
				NormalMax = 16.0,
				Comment = "ok",
				CreatedAt = DateTime.UtcNow
			}
		};

		var runner = new FakeQueryRunner(readRows);
		var svc = new LaboratoryQueryService(runner);

		// act
		var results = (await svc.GetAll(patientId)).ToList();

		// assert
		results.Should().HaveCount(1);
		results[0].ResultId.Should().Be(readRows[0].ResultId);
		results[0].TestType.Should().Be(readRows[0].TestType);
		results[0].Result.Should().BeApproximately(readRows[0].Result, 1e-6);
	}

	[Fact]
	public async Task GetByTestType_FiltersMappedRows()
	{
		// arrange
		var patientId = Guid.NewGuid();
		var readRows = new[]
		{
			new LaboratoryReadModel
			{
				ResultId = Guid.NewGuid(),
				PatientId = patientId,
				TestType = "Hemoglobin",
				Result = 13.2,
				NormalMin = 12.0,
				NormalMax = 16.0,
				Comment = "ok",
				CreatedAt = DateTime.UtcNow
			}
		};

		var runner = new FakeQueryRunner(readRows);
		var svc = new LaboratoryQueryService(runner);

		// act
		var results = (await svc.GetByTestType(patientId, "Hemoglobin")).ToList();

		// assert
		results.Should().HaveCount(1);
		results[0].TestType.Should().Be("Hemoglobin");
	}

	[Fact]
	public async Task GetByDateRange_MapsRowsToDomain()
	{
		// arrange
		var patientId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var readRows = new[]
		{
			new LaboratoryReadModel
			{
				ResultId = Guid.NewGuid(),
				PatientId = patientId,
				TestType = "Glucose",
				Result = 5.1,
				NormalMin = 3.9,
				NormalMax = 5.8,
				Comment = "fasting",
				CreatedAt = now.AddDays(-1)
			}
		};

		var runner = new FakeQueryRunner(readRows);
		var svc = new LaboratoryQueryService(runner);

		// act
		var results = (await svc.GetByDateRange(patientId, now.AddDays(-2), now)).ToList();

		// assert
		results.Should().HaveCount(1);
		results[0].ResultId.Should().Be(readRows[0].ResultId);
		// LaboratoryResult (domain) does not expose CreatedAt — assert mapped fields instead
		results[0].TestType.Should().Be(readRows[0].TestType);
	}

	[Fact]
	public async Task GetAll_ReturnsEmpty_WhenNoRows()
	{
		// arrange
		var patientId = Guid.NewGuid();
		var runner = new FakeQueryRunner(Array.Empty<object>());
		var svc = new LaboratoryQueryService(runner);

		// act
		var results = (await svc.GetAll(patientId)).ToList();

		// assert
		results.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAll_MapsMultipleRowsAndHandlesNullComment()
	{
		// arrange
		var patientId = Guid.NewGuid();
		var readRows = new[]
		{
			new LaboratoryReadModel
			{
				ResultId = Guid.NewGuid(),
				PatientId = patientId,
				TestType = "A1C",
				Result = 6.1,
				NormalMin = 4.0,
				NormalMax = 5.6,
				Comment = null,
				CreatedAt = DateTime.UtcNow
			},
			new LaboratoryReadModel
			{
				ResultId = Guid.NewGuid(),
				PatientId = patientId,
				TestType = "Cholesterol",
				Result = 180,
				NormalMin = 125,
				NormalMax = 200,
				Comment = "check diet",
				CreatedAt = DateTime.UtcNow
			}
		};

		var runner = new FakeQueryRunner(readRows);
		var svc = new LaboratoryQueryService(runner);

		// act
		var results = (await svc.GetAll(patientId)).ToList();

		// assert
		results.Should().HaveCount(2);
		results.Select(r => r.ResultId).Should().BeEquivalentTo(readRows.Select(rr => rr.ResultId));
		results.First(r => r.TestType == "A1C").Comment.Should().BeNull();
		results.First(r => r.TestType == "Cholesterol").Comment.Should().Be("check diet");
	}
}