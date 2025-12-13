using Med.Labs.Domain.Aggregates;


namespace Med.Labs.Domain.Snapshots;

public record LaboratorySnapshot(Guid PatientId, Dictionary<Guid, LaboratoryResult> Results, int Version);
