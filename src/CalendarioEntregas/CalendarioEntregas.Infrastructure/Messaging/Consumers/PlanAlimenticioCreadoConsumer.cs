using CalendarioEntregas.Application.Calendario.CreateCalendario;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents;
using Joselct.Communication.Contracts.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CalendarioEntregas.Infrastructure.Messaging.Consumers
{
	public class PlanAlimenticioCreadoConsumer : IIntegrationMessageConsumer<PlanAlimenticioCreadoIntegrationEvent>
	{
		private readonly ISender _sender;
		private readonly ILogger<PlanAlimenticioCreadoConsumer> _logger;

		public PlanAlimenticioCreadoConsumer(ISender sender, ILogger<PlanAlimenticioCreadoConsumer> logger)
		{
			_sender = sender;
			_logger = logger;
		}

		public async Task HandleAsync(PlanAlimenticioCreadoIntegrationEvent message, CancellationToken ct = default)
		{
			_logger.LogInformation(
				"Recibido PlanAlimenticioCreado: PlanId={PlanId}, PacienteId={PacienteId}, RequiereCatering={RequiereCatering}",
				message.PlanId, message.PacienteId, message.RequiereCatering);

			if (!message.RequiereCatering)
			{
				_logger.LogInformation(
					"Plan {PlanId} no requiere catering. No se crea calendario de entregas.",
					message.PlanId);
				return;
			}

			var fechaInicio = DateOnly.FromDateTime(message.FechaInicio);
			var fechaFin = fechaInicio.AddDays(message.Duracion);

			var command = new CreateCalendarioCommand(
				message.PacienteId,
				message.PlanId,
				fechaInicio,
				fechaFin
			);

			var result = await _sender.Send(command, ct);

			if (result.IsSuccess)
				_logger.LogInformation("Calendario creado: CalendarioId={CalendarioId}", result.Value);
			else
				_logger.LogError("Error al crear calendario para PlanId={PlanId}: {Error}", message.PlanId, result.Error!.Description);
		}
	}
}
