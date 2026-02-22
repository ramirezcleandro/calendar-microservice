using CalendarioEntregas.Application.Calendario.Queries;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.GetCalendario
{
    public record GetCalendarioQuery(Guid CalendarioId) : IRequest<Result<CalendarioDto>>;

    public class GetCalendarioHandler : IRequestHandler<GetCalendarioQuery, Result<CalendarioDto>>
    {
        private readonly ICalendarioEntregaRepository _repository;

        public GetCalendarioHandler(ICalendarioEntregaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<CalendarioDto>> Handle(GetCalendarioQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var calendario = await _repository.GetByIdAsync(request.CalendarioId);
                if (calendario == null)
                {
                    return Result<CalendarioDto>.Failure(
                        Error.ItemNotFound($"Calendario con ID {request.CalendarioId} no encontrado")
                    );
                }

                var direccionesDto = calendario.Direcciones.Select(d => new DireccionDto(
                    d.Id,
                    d.Fecha,
                    d.Direccion_Texto,
                    d.Referencias,
                    d.Latitud.Valor,
                    d.Longitud.Valor,
                    d.EsEntregaActiva,
                    d.ObtenerDiasRestantes()
                )).ToList();

                var calendarioDto = new CalendarioDto(
                    calendario.Id,
                    calendario.PacienteId,
                    calendario.PlanAlimenticioId,
                    calendario.FechaInicio,
                    calendario.FechaFin,
                    calendario.Activo,
                    calendario.ObtenerPorcentajeCompletado(),
                    direccionesDto
                );

                return Result<CalendarioDto>.Success(calendarioDto);
            }
            catch (Exception ex)
            {
                return Result<CalendarioDto>.Failure(
                    Error.Problem("Calendario.ConsultaError", $"Error al consultar calendario: {ex.Message}")
                );
            }
        }
    }
}
