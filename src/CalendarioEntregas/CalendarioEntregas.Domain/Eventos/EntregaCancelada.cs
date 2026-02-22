using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Domain.Eventos
{
    public record EntregaCancelada(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha
    ) : IDomainEvent;
}
