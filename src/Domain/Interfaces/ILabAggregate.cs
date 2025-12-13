using Med.Labs.Domain.Snapshots;

namespace Med.Labs.Domain.Interfaces;

public interface ILabAggregate
{
	void ApplyEvent(IDomainEvent @event);

	void AddResult(Guid patientId, Guid resultId, string testType, double result,
				   double normalMin, double normalMax, string? comment);

	void UpdateResult(Guid resultId, string testType, double result,
					  double normalMin, double normalMax, string? comment);

	void DeleteResult(Guid resultId);

	LaboratorySnapshot CreateSnapshot();

	void RestoreSnapshot(LaboratorySnapshot snapshot);
}
