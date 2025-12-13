using Xunit;
using Dapper;
using Med.Labs.Application.Handlers;
using Med.Labs.Application.Queries;
using Med.Labs.Domain.Commands;
using Med.Labs.Infrastructure.EventStore;
using Med.Labs.Infrastructure.Projections;
using Med.Labs.Infrastructure.Snapshots;
using Npgsql;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Med.Labs.IntegrationTests;

public class LaboratoryIntegrationTests : IAsyncLifetime
{
	private readonly string _connectionString = "Host=localhost;Port=5432;Database=med_labs;Username=med;Password=med";
	private readonly PostgresEventStore _eventStore;
	private readonly SnapshotStore _snapshotStore;
	private readonly LaboratoryProjectionHandler _projection;

	public LaboratoryIntegrationTests()
	{
		_eventStore = new PostgresEventStore(_connectionString, null!);
		_snapshotStore = new SnapshotStore(_connectionString);
		_projection = new LaboratoryProjectionHandler(_connectionString);
	}

	// IAsyncLifetime implementations — these must not be test methods.
	public async Task InitializeAsync()
	{
		// Clear database for fresh tests
		await using var conn = new NpgsqlConnection(_connectionString);
		await conn.ExecuteAsync("TRUNCATE TABLE event_store, snapshot_store, laboratory_read_model, outbox RESTART IDENTITY CASCADE");
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task Add_Update_Delete_LabResult_CrudFlow_Should_Work()
	{
		var patientId = Guid.NewGuid();
		var addHandler = new AddLaboratoryResultHandler(_eventStore, _snapshotStore, _projection);
		var runner = new DapperQueryRunner(() => new NpgsqlConnection(_connectionString));
		var queryService = new LaboratoryQueryService(runner);
		var updateHandler = new UpdateLaboratoryResultHandler(_eventStore, _snapshotStore, _projection);
		var deleteHandler = new DeleteLaboratoryResultHandler(_eventStore, _snapshotStore, _projection);

		// ADD
		var addCmd = new AddLaboratoryResultCommand(patientId, "HbA1c", 5.8, 4.0, 6.0, "Initial test");
		await addHandler.Handle(addCmd);

		var resultsAfterAdd = (await queryService.GetAll(patientId)).ToList();
		Assert.Single(resultsAfterAdd);
		var resultId = resultsAfterAdd.First().ResultId;

		// UPDATE
		var updateCmd = new UpdateLaboratoryResultCommand(resultId, patientId, "HbA1c", 6.2, 4.0, 6.0, "Follow-up test");
		await updateHandler.Handle(updateCmd);

		var resultsAfterUpdate = (await queryService.GetAll(patientId)).ToList();
		Assert.Single(resultsAfterUpdate);
		Assert.Equal(6.2, resultsAfterUpdate.First().Result);

		// DELETE
		var deleteCmd = new DeleteLaboratoryResultCommand(patientId, resultId);
		await deleteHandler.Handle(deleteCmd);

		var resultsAfterDelete = (await queryService.GetAll(patientId)).ToList();
		Assert.Empty(resultsAfterDelete);
	}

	[Fact]
	public async Task QueryByTestType_And_DateRange_Should_Work()
	{
		var patientId = Guid.NewGuid();
		var addHandler = new AddLaboratoryResultHandler(_eventStore, _snapshotStore, _projection);
		var runner = new DapperQueryRunner(() => new NpgsqlConnection(_connectionString));
		var queryService = new LaboratoryQueryService(runner);

		// Add multiple results
		await addHandler.Handle(new AddLaboratoryResultCommand(patientId, "HbA1c", 5.5, 4, 6, null));
		await Task.Delay(50); // ensure timestamp difference
		await addHandler.Handle(new AddLaboratoryResultCommand(patientId, "Cholesterol", 4.8, 3, 5, null));

		var hba1cResults = (await queryService.GetByTestType(patientId, "HbA1c")).ToList();
		Assert.Single(hba1cResults);

		var from = DateTime.UtcNow.AddMinutes(-5);
		var to = DateTime.UtcNow.AddMinutes(5);
		var dateResults = (await queryService.GetByDateRange(patientId, from, to)).ToList();
		Assert.Equal(2, dateResults.Count);
	}
}