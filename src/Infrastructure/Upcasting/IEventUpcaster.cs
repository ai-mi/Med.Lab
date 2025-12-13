using Med.Labs.Domain.Interfaces;

namespace Med.Labs.Infrastructure.Upcasting;

public interface IEventUpcaster
{
	bool CanUpcast(IDomainEvent @event);
	IDomainEvent Upcast(IDomainEvent @event);
}
