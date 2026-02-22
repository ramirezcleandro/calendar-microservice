using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.AgregarDireccion
{
    public record AgregarDireccionCommand(
        Guid CalendarioId,
        DateOnly Fecha,
        string Direccion,
        string Referencias,
        double Latitud,
        double Longitud
    ) : IRequest<Result<Guid>>;

    public class AgregarDireccionHandler : IRequestHandler<AgregarDireccionCommand, Result<Guid>>
    {
        private readonly ICalendarioEntregaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AgregarDireccionHandler(ICalendarioEntregaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(AgregarDireccionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var calendario = await _repository.GetByIdAsync(request.CalendarioId);
                if (calendario == null)
                {
                    return Result<Guid>.Failure(
                        Error.ItemNotFound($"Calendario con ID {request.CalendarioId} no encontrado")
                    );
                }

                var latitud = new Latitud(request.Latitud);
                var longitud = new Longitud(request.Longitud);

                calendario.AgregarDireccion(
                    request.Fecha,
                    request.Direccion,
                    request.Referencias,
                    latitud,
                    longitud
                );

                await _repository.UpdateAsync(calendario);
                await _unitOfWork.CommitAsync(cancellationToken);

                var direccion = calendario.ObtenerDireccion(request.Fecha);
                return Result<Guid>.Success(direccion!.Id);
            }
            catch (InvalidOperationException ex)
            {
                return Result<Guid>.Failure(
                    Error.Problem("Calendario.DireccionError", ex.Message)
                );
            }
            catch (ArgumentException ex)
            {
                return Result<Guid>.Failure(
                    Error.Problem("Calendario.ValidacionError", ex.Message)
                );
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "Sin detalles";
                return Result<Guid>.Failure(
                    Error.Problem("Calendario.Error", $"{ex.Message}. Inner: {inner}")
                );
            }
        }
    }
}
