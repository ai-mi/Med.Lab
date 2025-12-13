namespace Med.Labs.Application.Queries;

public class LaboratoryReadModel
{
	public Guid ResultId { get; set; }
	public Guid PatientId { get; set; }
	public string TestType { get; set; } = string.Empty;
	public double Result { get; set; }
	public double NormalMin { get; set; }
	public double NormalMax { get; set; }
	public string? Comment { get; set; }
	public DateTime CreatedAt { get; set; }
}