using CalendarioEntregas.Application.Calendario.GetCalendario;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
	public class GetCalendarioHandlerTests
	{
		private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
		private readonly GetCalendarioHandler _handler;

		public GetCalendarioHandlerTests()
		{
			_repositoryMock = new Mock<ICalendarioEntregaRepository>();
			_handler = new GetCalendarioHandler(_repositoryMock.Object);
		}

		[Fact]
		public async Task Handle_DebeRetornarCalendarioConDirecciones()
		{
			var calendario = new CalendarioEntrega(
				Guid.NewGuid(),
				Guid.NewGuid(),
				DateOnly.FromDateTime(DateTime.Today),
				DateOnly.FromDateTime(DateTime.Today.AddDays(30))
			);

			var fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
			calendario.AgregarDireccion(fecha, "Av. Test 123", "Ref", new Latitud(-12.0), new Longitud(-77.0));

			_repositoryMock
				.Setup(r => r.GetByIdAsync(calendario.Id))
				.ReturnsAsync(calendario);

			var result = await _handler.Handle(
				new GetCalendarioQuery(calendario.Id),
				CancellationToken.None
			);

			result.IsSuccess.Should().BeTrue();
			result.Value!.Id.Should().Be(calendario.Id);
			result.Value.PacienteId.Should().Be(calendario.PacienteId);
			result.Value.PlanAlimenticioId.Should().Be(calendario.PlanAlimenticioId);
			result.Value.Direcciones.Should().HaveCount(1);
			result.Value.Direcciones.First().Direccion.Should().Be("Av. Test 123");
		}

		[Fact]
		public async Task Handle_DebeFallarSiCalendarioNoExiste()
		{
			var calendarioId = Guid.NewGuid();
			_repositoryMock
				.Setup(r => r.GetByIdAsync(calendarioId))
				.ReturnsAsync((CalendarioEntrega?)null);

			var result = await _handler.Handle(
				new GetCalendarioQuery(calendarioId),
				CancellationToken.None
			);

			result.IsFailure.Should().BeTrue();
			result.Error.Code.Should().Be("ItemNotFound");
		}

		[Fact]
		public async Task Handle_DebeRetornarFailureSiRepositorioLanzaExcepcion()
		{
			var calendarioId = Guid.NewGuid();
			_repositoryMock
				.Setup(r => r.GetByIdAsync(calendarioId))
				.ThrowsAsync(new InvalidOperationException("db caída"));

			var result = await _handler.Handle(
				new GetCalendarioQuery(calendarioId),
				CancellationToken.None
			);

			result.IsFailure.Should().BeTrue();
			result.Error.Code.Should().Be("Calendario.ConsultaError");
		}
	}
}
