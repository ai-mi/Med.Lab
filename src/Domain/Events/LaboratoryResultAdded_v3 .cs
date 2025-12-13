using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Domain.Events;

public class LaboratoryResultAdded_v3 : IDomainEvent
{
	public Guid PatientId { get; }
	public Guid ResultId { get; }
	public string TestType { get; }
	public double Result { get; }
	public double NormalMin { get; }
	public double NormalMax { get; }
	public string? Comment { get; }

	public LaboratoryResultAdded_v3(Guid patientId, Guid resultId, string testType,
									double result, double normalMin, double normalMax, string? comment)
	{
		PatientId = patientId;
		ResultId = resultId;
		TestType = testType;
		Result = result;
		NormalMin = normalMin;
		NormalMax = normalMax;
		Comment = comment;
	}
}
