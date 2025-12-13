using Med.Labs.Domain.Events;
using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Infrastructure.Upcasting;

public class LabEventUpcaster_v1_to_v2 : IEventUpcaster
{
	public bool CanUpcast(IDomainEvent @event) => @event is LaboratoryResultAdded_v1;

	public IDomainEvent Upcast(IDomainEvent @event)
	{
		if (@event is LaboratoryResultAdded_v1 e)
		{
			return new LaboratoryResultAdded_v2(e.PatientId, e.ResultId, e.TestType, e.Result, 0, 0);
		}
		return @event;
	}
}
