using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Domain.Eventos
{
    public record CalendarioCreado(
        Guid CalendarioId,
        Guid PacienteId,
        Guid PlanAlimenticioId,
        DateOnly FechaInicio,
        DateOnly FechaFin
    ) : IDomainEvent;
}
