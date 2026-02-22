using CalendarioEntregas.Application.Calendario.Queries;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.ObtenerProximaEntrega
{
    public record ObtenerProximaEntregaQuery(Guid CalendarioId) : IRequest<Result<DireccionDto?>>;

    public class ObtenerProximaEntregaHandler : IRequestHandler<ObtenerProximaEntregaQuery, Result<DireccionDto?>>
    {
        private readonly ICalendarioEntregaRepository _repository;

        public ObtenerProximaEntregaHandler(ICalendarioEntregaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<DireccionDto?>> Handle(ObtenerProximaEntregaQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var calendario = await _repository.GetByIdAsync(request.CalendarioId);
                if (calendario == null)
                {
                    return Result<DireccionDto?>.Failure(
                        Error.ItemNotFound($"Calendario con ID {request.CalendarioId} no encontrado")
                    );
                }

                var proximaEntrega = calendario.ObtenerProximaEntrega();

                if (proximaEntrega == null)
                {
                    return Result<DireccionDto?>.Success(null);
                }

                var direccionDto = new DireccionDto(
                    proximaEntrega.Id,
                    proximaEntrega.Fecha,
                    proximaEntrega.Direccion_Texto,
                    proximaEntrega.Referencias,
                    proximaEntrega.Latitud.Valor,
                    proximaEntrega.Longitud.Valor,
                    proximaEntrega.EsEntregaActiva,
                    proximaEntrega.ObtenerDiasRestantes()
                );

                return Result<DireccionDto?>.Success(direccionDto);
            }
            catch (Exception ex)
            {
                return Result<DireccionDto?>.Failure(
                    Error.Problem("Calendario.ConsultaError", $"Error al obtener próxima entrega: {ex.Message}")
                );
            }
        }
    }
}
