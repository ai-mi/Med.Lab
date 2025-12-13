namespace Med.Labs.Domain.Commands;

public record AddLaboratoryResultCommand(
	Guid PatientId,
	string TestType,
	double Result,
	double NormalMin,
	double NormalMax,
	string? Comment
);
