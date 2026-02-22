using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.ModificarDireccion
{
    public record ModificarDireccionCommand(
        Guid CalendarioId,
        DateOnly Fecha,
        string NuevaDireccion,
        string Referencias,
        double Latitud,
        double Longitud
    ) : IRequest<Result<Unit>>;

    public class ModificarDireccionHandler : IRequestHandler<ModificarDireccionCommand, Result<Unit>>
    {
        private readonly ICalendarioEntregaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ModificarDireccionHandler(ICalendarioEntregaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(ModificarDireccionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var calendario = await _repository.GetByIdAsync(request.CalendarioId);
                if (calendario == null)
                {
                    return Result<Unit>.Failure(
                        Error.ItemNotFound($"Calendario con ID {request.CalendarioId} no encontrado")
                    );
                }

                var direccion = calendario.ObtenerDireccion(request.Fecha);
                if (direccion == null)
                {
                    return Result<Unit>.Failure(
                        Error.ItemNotFound($"No existe dirección para la fecha {request.Fecha}")
                    );
                }

                var latitud = new Latitud(request.Latitud);
                var longitud = new Longitud(request.Longitud);

                direccion.Modificar(
                    request.NuevaDireccion,
                    request.Referencias,
                    latitud,
                    longitud
                );

                await _repository.UpdateAsync(calendario);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (InvalidOperationException ex)
            {
                return Result<Unit>.Failure(
                    Error.Problem("Calendario.ModificacionError", ex.Message)
                );
            }
            catch (ArgumentException ex)
            {
                return Result<Unit>.Failure(
                    Error.Problem("Calendario.ValidacionError", ex.Message)
                );
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "Sin detalles";
                return Result<Unit>.Failure(
                    Error.Problem("Calendario.Error", $"{ex.Message}. Inner: {inner}")
                );
            }
        }
    }
}
