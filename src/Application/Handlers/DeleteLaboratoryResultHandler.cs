using Med.Labs.Domain.Aggregates;
using Med.Labs.Domain.Commands;
using Med.Labs.Infrastructure.EventStore;
using Med.Labs.Infrastructure.Projections;
using Med.Labs.Infrastructure.Snapshots;

namespace Med.Labs.Application.Handlers;

public class DeleteLaboratoryResultHandler
{
	private readonly PostgresEventStore _eventStore;
	private readonly SnapshotStore _snapshotStore;
	private readonly LaboratoryProjectionHandler _projection;

	public DeleteLaboratoryResultHandler(
		PostgresEventStore eventStore,
		SnapshotStore snapshotStore,
		LaboratoryProjectionHandler projection)
	{
		_eventStore = eventStore;
		_snapshotStore = snapshotStore;
		_projection = projection;
	}

	public async Task Handle(DeleteLaboratoryResultCommand cmd)
	{
		var aggregate = await LoadAggregate(cmd.PatientId);
		aggregate.DeleteResult(cmd.ResultId);

		foreach (var @event in aggregate.GetUncommittedEvents())
		{
			await _eventStore.AppendEvent(aggregate.PatientId, @event);
			await _projection.Project(@event);
		}

		var snapshot = aggregate.CreateSnapshot();
		await _snapshotStore.SaveSnapshot(snapshot);

		aggregate.ClearUncommittedEvents();
	}

	private async Task<LaboratoryResultAggregate> LoadAggregate(Guid patientId)
	{
		var snapshot = await _snapshotStore.LoadSnapshot(patientId);
		var aggregate = new LaboratoryResultAggregate();

		if (snapshot != null)
			aggregate.RestoreSnapshot(snapshot);

		var events = await _eventStore.LoadEvents(patientId, aggregate.Version);
		foreach (var @event in events)
			aggregate.ApplyEvent(@event);

		return aggregate;
	}
}
