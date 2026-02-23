using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.ReactivarEntrega
{
    public record ReactivarEntregaCommand(
        Guid CalendarioId,
        DateOnly Fecha
    ) : IRequest<Result<Unit>>;

    public class ReactivarEntregaHandler : IRequestHandler<ReactivarEntregaCommand, Result<Unit>>
    {
        private readonly ICalendarioEntregaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ReactivarEntregaHandler(ICalendarioEntregaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(ReactivarEntregaCommand request, CancellationToken cancellationToken)
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

                calendario.ReactivarEntrega(request.Fecha);
                await _repository.UpdateAsync(calendario);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (InvalidOperationException ex)
            {
                return Result<Unit>.Failure(
                    Error.Problem("Calendario.ReactivacionError", ex.Message)
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
