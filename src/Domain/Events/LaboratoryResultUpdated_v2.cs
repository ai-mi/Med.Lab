using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Domain.Events;

public class LaboratoryResultUpdated_v2 : IDomainEvent
{
	public Guid PatientId { get; }
	public Guid ResultId { get; }
	public string TestType { get; }
	public double Result { get; }
	public double NormalMin { get; }
	public double NormalMax { get; }
	public string? Comment { get; }

	public LaboratoryResultUpdated_v2(Guid resultId, Guid patientId, string testType,
									 double result, double normalMin, double normalMax, string? comment)
	{
		ResultId = resultId;
		PatientId = patientId;
		TestType = testType;
		Result = result;
		NormalMin = normalMin;
		NormalMax = normalMax;
		Comment = comment;
	}
}
