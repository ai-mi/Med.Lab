using Med.Labs.Domain.Events;
using Med.Labs.Domain.Interfaces;
using Med.Labs.Domain.Snapshots;

namespace Med.Labs.Domain.Aggregates;

public class LaboratoryResultAggregate
{
	public Guid PatientId { get; private set; }
	public Dictionary<Guid, LaboratoryResult> Results { get; private set; } = new();
	public int Version { get; private set; } = 0;

	private readonly List<IDomainEvent> _uncommittedEvents = new();

	public IEnumerable<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();
	public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

	public void ApplyEvent(IDomainEvent @event)
	{
		switch (@event)
		{
			case LaboratoryResultAdded_v1 e1:
				Results[e1.ResultId] = new LaboratoryResult(
					e1.ResultId, e1.TestType, e1.Result, 0, 0, null);
				PatientId = e1.PatientId;
				Version++;
				break;

			case LaboratoryResultAdded_v2 e2:
				Results[e2.ResultId] = new LaboratoryResult(
					e2.ResultId, e2.TestType, e2.Result, e2.NormalMin, e2.NormalMax, null);
				PatientId = e2.PatientId;
				Version++;
				break;

			case LaboratoryResultAdded_v3 e3:
				Results[e3.ResultId] = new LaboratoryResult(
					e3.ResultId, e3.TestType, e3.Result, e3.NormalMin, e3.NormalMax, e3.Comment);
				PatientId = e3.PatientId;
				Version++;
				break;

			case LaboratoryResultUpdated_v1 u1:
				if (Results.ContainsKey(u1.ResultId))
				{
					Results[u1.ResultId] = new LaboratoryResult(
						u1.ResultId, u1.TestType, u1.Result, 0, 0, null);
				}
				Version++;
				break;

			case LaboratoryResultUpdated_v2 u2:
				if (Results.ContainsKey(u2.ResultId))
				{
					Results[u2.ResultId] = new LaboratoryResult(
						u2.ResultId, u2.TestType, u2.Result, u2.NormalMin, u2.NormalMax, u2.Comment);
				}
				Version++;
				break;

			case LaboratoryResultDeleted_v1 d1:
				Results.Remove(d1.ResultId);
				Version++;
				break;
		}
	}

	public void AddResult(Guid patientId, Guid resultId, string testType, double result,
						  double normalMin, double normalMax, string? comment)
	{
		if (Results.ContainsKey(resultId))
			throw new InvalidOperationException("Result already exists");

		var @event = new LaboratoryResultAdded_v3(patientId, resultId, testType, result, normalMin, normalMax, comment);
		_uncommittedEvents.Add(@event);
		ApplyEvent(@event);
	}

	public void UpdateResult(Guid resultId, string testType, double result,
							 double normalMin, double normalMax, string? comment)
	{
		if (!Results.ContainsKey(resultId))
			throw new InvalidOperationException("Result does not exist");

		var @event = new LaboratoryResultUpdated_v2(resultId, PatientId, testType, result, normalMin, normalMax, comment);
		_uncommittedEvents.Add(@event);
		ApplyEvent(@event);
	}

	public void DeleteResult(Guid resultId)
	{
		if (!Results.ContainsKey(resultId))
			throw new InvalidOperationException("Result does not exist");

		var @event = new LaboratoryResultDeleted_v1(PatientId, resultId);
		_uncommittedEvents.Add(@event);
		ApplyEvent(@event);
	}

	public LaboratorySnapshot CreateSnapshot()
	{
		// return a copy to avoid external mutation of the aggregate state
		return new LaboratorySnapshot(PatientId, new Dictionary<Guid, LaboratoryResult>(Results), Version);
	}

	public void RestoreSnapshot(LaboratorySnapshot snapshot)
	{
		PatientId = snapshot.PatientId;
		Results = new Dictionary<Guid, LaboratoryResult>(snapshot.Results);
		Version = snapshot.Version;
	}
}

public record LaboratoryResult(Guid ResultId, string TestType, double Result, double NormalMin, double NormalMax, string? Comment);