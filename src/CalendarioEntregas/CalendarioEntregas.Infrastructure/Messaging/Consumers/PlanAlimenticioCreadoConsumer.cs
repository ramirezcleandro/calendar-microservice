using CalendarioEntregas.Application.Calendario.CreateCalendario;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CalendarioEntregas.Infrastructure.Messaging.Consumers
{
    /// <summary>
    /// Consumer que escucha el evento publicado por el microservicio de Planes Alimenticios.
    /// Cuando se crea un plan, automáticamente se crea el calendario de entregas.
    /// </summary>
    public class PlanAlimenticioCreadoConsumer : IConsumer<PlanAlimenticioCreadoIntegrationEvent>
    {
        private readonly ISender _sender;
        private readonly ILogger<PlanAlimenticioCreadoConsumer> _logger;

        public PlanAlimenticioCreadoConsumer(ISender sender, ILogger<PlanAlimenticioCreadoConsumer> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PlanAlimenticioCreadoIntegrationEvent> context)
        {
            var evento = context.Message;

            _logger.LogInformation(
                "Recibido PlanAlimenticioCreado: PlanId={PlanId}, PacienteId={PacienteId}",
                evento.PlanAlimenticioId, evento.PacienteId);

            var command = new CreateCalendarioCommand(
                evento.PacienteId,
                evento.PlanAlimenticioId,
                evento.FechaInicio,
                evento.FechaFin
            );

            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Calendario creado automáticamente: CalendarioId={CalendarioId}",
                    result.Value);
            }
            else
            {
                _logger.LogError(
                    "Error al crear calendario para PlanId={PlanId}: {Error}",
                    evento.PlanAlimenticioId, result.Error!.Description);
            }
        }
    }
}
