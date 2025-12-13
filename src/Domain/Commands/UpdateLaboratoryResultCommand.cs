namespace Med.Labs.Domain.Commands;

public record UpdateLaboratoryResultCommand(
	Guid ResultId,
	Guid PatientId,
	string TestType,
	double Result,
	double NormalMin,
	double NormalMax,
	string? Comment
);
