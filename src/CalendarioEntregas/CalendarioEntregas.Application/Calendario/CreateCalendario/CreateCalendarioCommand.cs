using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.CreateCalendario
{
    public record CreateCalendarioCommand(
        Guid PacienteId,
        Guid PlanAlimenticioId,
        DateOnly FechaInicio,
        DateOnly FechaFin
    ) : IRequest<Result<Guid>>;
}
