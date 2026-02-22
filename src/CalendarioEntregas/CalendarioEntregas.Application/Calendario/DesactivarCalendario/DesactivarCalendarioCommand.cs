using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.DesactivarCalendario
{
    public record DesactivarCalendarioCommand(Guid CalendarioId) : IRequest<Result<Unit>>;

    public class DesactivarCalendarioHandler : IRequestHandler<DesactivarCalendarioCommand, Result<Unit>>
    {
        private readonly ICalendarioEntregaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DesactivarCalendarioHandler(ICalendarioEntregaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(DesactivarCalendarioCommand request, CancellationToken cancellationToken)
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

                if (!calendario.Activo)
                {
                    return Result<Unit>.Failure(
                        Error.Problem("Calendario.YaDesactivado", "El calendario ya está desactivado")
                    );
                }

                calendario.Desactivar();

                await _repository.UpdateAsync(calendario);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
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
