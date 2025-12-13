namespace Med.Labs.Domain.Commands;

public record DeleteLaboratoryResultCommand(Guid PatientId, Guid ResultId);
