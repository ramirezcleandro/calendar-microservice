using CalendarioEntregas.Application.Calendario.CreateCalendario;
using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Infrastructure.Messaging.Consumers;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
	public class PlanAlimenticioCreadoConsumerTests
	{
		private readonly Mock<ISender> _senderMock;
		private readonly PlanAlimenticioCreadoConsumer _consumer;

		public PlanAlimenticioCreadoConsumerTests()
		{
			_senderMock = new Mock<ISender>();
			_consumer = new PlanAlimenticioCreadoConsumer(
				_senderMock.Object,
				NullLogger<PlanAlimenticioCreadoConsumer>.Instance
			);
		}

		private static PlanAlimenticioCreadoIntegrationEvent NuevoEvento(bool requiereCatering) =>
			new(
				PlanId: Guid.NewGuid(),
				PacienteId: Guid.NewGuid(),
				NutricionistaId: Guid.NewGuid(),
				FechaInicio: DateTime.Today,
				Duracion: 30,
				RequiereCatering: requiereCatering
			);

		[Fact]
		public async Task HandleAsync_SiRequiereCateringEsFalse_NoDebeCrearCalendario()
		{
			var evento = NuevoEvento(requiereCatering: false);

			await _consumer.HandleAsync(evento);

			_senderMock.Verify(
				s => s.Send(It.IsAny<CreateCalendarioCommand>(), It.IsAny<CancellationToken>()),
				Times.Never);
		}

		[Fact]
		public async Task HandleAsync_SiRequiereCateringEsTrue_DebeEnviarCreateCalendarioCommand()
		{
			var evento = NuevoEvento(requiereCatering: true);
			_senderMock
				.Setup(s => s.Send(It.IsAny<CreateCalendarioCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

			await _consumer.HandleAsync(evento);

			_senderMock.Verify(
				s => s.Send(
					It.Is<CreateCalendarioCommand>(cmd =>
						cmd.PacienteId == evento.PacienteId &&
						cmd.PlanAlimenticioId == evento.PlanId),
					It.IsAny<CancellationToken>()),
				Times.Once);
		}

		[Fact]
		public async Task HandleAsync_PorDefaultLosEventosAntiguos_DebenCrearCalendario()
		{
			// Mensajes de versiones anteriores del productor no traen el flag;
			// el record aplica el default (true) para preservar el comportamiento.
			var evento = new PlanAlimenticioCreadoIntegrationEvent(
				PlanId: Guid.NewGuid(),
				PacienteId: Guid.NewGuid(),
				NutricionistaId: Guid.NewGuid(),
				FechaInicio: DateTime.Today,
				Duracion: 30
			);
			evento.RequiereCatering.Should().BeTrue();

			_senderMock
				.Setup(s => s.Send(It.IsAny<CreateCalendarioCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

			await _consumer.HandleAsync(evento);

			_senderMock.Verify(
				s => s.Send(It.IsAny<CreateCalendarioCommand>(), It.IsAny<CancellationToken>()),
				Times.Once);
		}
	}
}
