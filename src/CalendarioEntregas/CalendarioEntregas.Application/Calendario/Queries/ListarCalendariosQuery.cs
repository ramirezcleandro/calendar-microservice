using CalendarioEntregas.Application.Calendario.Queries;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.ListarCalendarios
{
    public record ListarCalendariosQuery(Guid? PacienteId = null) : IRequest<Result<IEnumerable<CalendarioDto>>>;

    public class ListarCalendariosHandler : IRequestHandler<ListarCalendariosQuery, Result<IEnumerable<CalendarioDto>>>
    {
        private readonly ICalendarioEntregaRepository _repository;

        public ListarCalendariosHandler(ICalendarioEntregaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<CalendarioDto>>> Handle(ListarCalendariosQuery request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Domain.Agregados.CalendarioEntrega> calendarios;

                if (request.PacienteId.HasValue)
                {
                    var calendario = await _repository.GetByPacienteIdAsync(request.PacienteId.Value);
                    calendarios = calendario != null ? new[] { calendario } : Array.Empty<Domain.Agregados.CalendarioEntrega>();
                }
                else
                {
                    calendarios = await _repository.GetAllAsync();
                }

                var calendariosDtos = calendarios.Select(c => new CalendarioDto(
                    c.Id,
                    c.PacienteId,
                    c.PlanAlimenticioId,
                    c.FechaInicio,
                    c.FechaFin,
                    c.Activo,
                    c.ObtenerPorcentajeCompletado(),
                    c.Direcciones.Select(d => new DireccionDto(
                        d.Id,
                        d.Fecha,
                        d.Direccion_Texto,
                        d.Referencias,
                        d.Latitud.Valor,
                        d.Longitud.Valor,
                        d.EsEntregaActiva,
                        d.ObtenerDiasRestantes()
                    )).ToList()
                )).ToList();

                return Result<IEnumerable<CalendarioDto>>.Success((IEnumerable<CalendarioDto>)calendariosDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CalendarioDto>>.Failure(
                    Error.Problem("Calendario.ListadoError", $"Error al listar calendarios: {ex.Message}")
                );
            }
        }
    }
}
