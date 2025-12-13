using Med.Labs.Domain.Events;
using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Infrastructure.Upcasting;

public class LabEventUpcaster_v2_to_v3 : IEventUpcaster
{
	public bool CanUpcast(IDomainEvent @event) => @event is LaboratoryResultAdded_v2;

	public IDomainEvent Upcast(IDomainEvent @event)
	{
		if (@event is LaboratoryResultAdded_v2 e)
		{
			return new LaboratoryResultAdded_v3(e.PatientId, e.ResultId, e.TestType, e.Result, e.NormalMin, e.NormalMax, null);
		}
		return @event;
	}
}
