using CalendarioEntregas.Application.Calendario.Queries;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.ObtenerDireccionesActivas
{
    public record ObtenerDireccionesActivasQuery(Guid CalendarioId) : IRequest<Result<IEnumerable<DireccionDto>>>;

    public class ObtenerDireccionesActivasHandler : IRequestHandler<ObtenerDireccionesActivasQuery, Result<IEnumerable<DireccionDto>>>
    {
        private readonly ICalendarioEntregaRepository _repository;

        public ObtenerDireccionesActivasHandler(ICalendarioEntregaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<DireccionDto>>> Handle(ObtenerDireccionesActivasQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var calendario = await _repository.GetByIdAsync(request.CalendarioId);
                if (calendario == null)
                {
                    return Result<IEnumerable<DireccionDto>>.Failure(
                        Error.ItemNotFound($"Calendario con ID {request.CalendarioId} no encontrado")
                    );
                }

                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var direccionesActivas = calendario.ObtenerDireccionesActivas(hoy);

                var direccionesDtos = direccionesActivas.Select(d => new DireccionDto(
                    d.Id,
                    d.Fecha,
                    d.Direccion_Texto,
                    d.Referencias,
                    d.Latitud.Valor,
                    d.Longitud.Valor,
                    d.EsEntregaActiva,
                    d.ObtenerDiasRestantes()
                )).ToList();

                return Result<IEnumerable<DireccionDto>>.Success(direccionesDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<DireccionDto>>.Failure(
                    Error.Problem("Calendario.ConsultaError", $"Error al obtener direcciones activas: {ex.Message}")
                );
            }
        }
    }
}
