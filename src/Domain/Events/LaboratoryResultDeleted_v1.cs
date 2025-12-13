using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Domain.Events;

public class LaboratoryResultDeleted_v1 : IDomainEvent
{
	public Guid PatientId { get; }
	public Guid ResultId { get; }

	public LaboratoryResultDeleted_v1(Guid patientId, Guid resultId)
	{
		PatientId = patientId;
		ResultId = resultId;
	}
}
