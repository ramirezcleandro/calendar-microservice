using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Domain.Eventos
{
    public record EntregaReactivada(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha
    ) : IDomainEvent;
}
