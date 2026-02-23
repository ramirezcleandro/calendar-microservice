using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.MarcarDiaNoEntrega
{
    public record MarcarDiaNoEntregaCommand(
        Guid CalendarioId,
        DateOnly Fecha
    ) : IRequest<Result<Unit>>;

    public class MarcarDiaNoEntregaHandler : IRequestHandler<MarcarDiaNoEntregaCommand, Result<Unit>>
    {
        private readonly ICalendarioEntregaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public MarcarDiaNoEntregaHandler(ICalendarioEntregaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(MarcarDiaNoEntregaCommand request, CancellationToken cancellationToken)
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

                calendario.MarcarDiaNoEntrega(request.Fecha);

                await _repository.UpdateAsync(calendario);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (InvalidOperationException ex)
            {
                return Result<Unit>.Failure(
                    Error.Problem("Calendario.CancelacionError", ex.Message)
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
