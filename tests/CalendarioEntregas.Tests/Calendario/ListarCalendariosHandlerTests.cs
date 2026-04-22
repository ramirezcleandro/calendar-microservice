using CalendarioEntregas.Application.Calendario.ListarCalendarios;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
	public class ListarCalendariosHandlerTests
	{
		private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
		private readonly ListarCalendariosHandler _handler;

		public ListarCalendariosHandlerTests()
		{
			_repositoryMock = new Mock<ICalendarioEntregaRepository>();
			_handler = new ListarCalendariosHandler(_repositoryMock.Object);
		}

		private static CalendarioEntrega NuevoCalendario(Guid? pacienteId = null) =>
			new(
				pacienteId ?? Guid.NewGuid(),
				Guid.NewGuid(),
				DateOnly.FromDateTime(DateTime.Today),
				DateOnly.FromDateTime(DateTime.Today.AddDays(30))
			);

		[Fact]
		public async Task Handle_SinPacienteId_DebeRetornarTodosLosCalendarios()
		{
			var calendarios = new[] { NuevoCalendario(), NuevoCalendario(), NuevoCalendario() };
			_repositoryMock
				.Setup(r => r.GetAllAsync())
				.ReturnsAsync(calendarios);

			var result = await _handler.Handle(
				new ListarCalendariosQuery(),
				CancellationToken.None
			);

			result.IsSuccess.Should().BeTrue();
			result.Value.Should().HaveCount(3);
			_repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
		}

		[Fact]
		public async Task Handle_ConPacienteId_DebeRetornarSoloCalendarioDelPaciente()
		{
			var pacienteId = Guid.NewGuid();
			var calendario = NuevoCalendario(pacienteId);
			_repositoryMock
				.Setup(r => r.GetByPacienteIdAsync(pacienteId))
				.ReturnsAsync(calendario);

			var result = await _handler.Handle(
				new ListarCalendariosQuery(pacienteId),
				CancellationToken.None
			);

			result.IsSuccess.Should().BeTrue();
			result.Value.Should().HaveCount(1);
			result.Value!.First().PacienteId.Should().Be(pacienteId);
			_repositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
		}

		[Fact]
		public async Task Handle_ConPacienteIdSinCalendario_DebeRetornarListaVacia()
		{
			var pacienteId = Guid.NewGuid();
			_repositoryMock
				.Setup(r => r.GetByPacienteIdAsync(pacienteId))
				.ReturnsAsync((CalendarioEntrega?)null);

			var result = await _handler.Handle(
				new ListarCalendariosQuery(pacienteId),
				CancellationToken.None
			);

			result.IsSuccess.Should().BeTrue();
			result.Value.Should().BeEmpty();
		}

		[Fact]
		public async Task Handle_DebeRetornarFailureSiRepositorioLanzaExcepcion()
		{
			_repositoryMock
				.Setup(r => r.GetAllAsync())
				.ThrowsAsync(new InvalidOperationException("db caída"));

			var result = await _handler.Handle(
				new ListarCalendariosQuery(),
				CancellationToken.None
			);

			result.IsFailure.Should().BeTrue();
			result.Error.Code.Should().Be("Calendario.ListadoError");
		}
	}
}
