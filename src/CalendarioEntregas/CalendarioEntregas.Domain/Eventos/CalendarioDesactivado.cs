using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Domain.Eventos
{
    public record CalendarioDesactivado(
        Guid CalendarioId,
        Guid PacienteId
    ) : IDomainEvent;
}
