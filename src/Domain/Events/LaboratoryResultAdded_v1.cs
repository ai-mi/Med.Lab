using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Domain.Events;

public class LaboratoryResultAdded_v1 : IDomainEvent
{
	public Guid PatientId { get; }
	public Guid ResultId { get; }
	public string TestType { get; }
	public double Result { get; }

	public LaboratoryResultAdded_v1(Guid patientId, Guid resultId, string testType, double result)
	{
		PatientId = patientId;
		ResultId = resultId;
		TestType = testType;
		Result = result;
	}
}

