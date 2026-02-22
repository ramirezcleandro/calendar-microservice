using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Domain.Repositories;
using MediatR;

namespace CalendarioEntregas.Application.Calendario.CreateCalendario
{
    public class CreateCalendarioHandler : IRequestHandler<CreateCalendarioCommand, Result<Guid>>
    {
        private readonly ICalendarioEntregaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCalendarioHandler(ICalendarioEntregaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateCalendarioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var calendario = new CalendarioEntrega(
                    request.PacienteId,
                    request.PlanAlimenticioId,
                    request.FechaInicio,
                    request.FechaFin
                );

                await _repository.AddAsync(calendario);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result<Guid>.Success(calendario.Id);
            }
            catch (ArgumentException ex)
            {
                var error = Error.Problem(
                    "Calendario.CreacionError",
                    $"Error de validación al crear calendario: {ex.Message}"
                );
                return Result<Guid>.Failure(error);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "Sin detalles adicionales";
                var error = Error.Problem(
                    "Calendario.CreacionError",
                    $"Error al crear calendario: {ex.Message}. Inner: {inner}"
                );
                return Result<Guid>.Failure(error);
            }
        }
    }
}
