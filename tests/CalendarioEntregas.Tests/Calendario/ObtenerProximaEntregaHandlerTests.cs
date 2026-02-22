using CalendarioEntregas.Application.Calendario.ObtenerProximaEntrega;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
    public class ObtenerProximaEntregaHandlerTests
    {
        private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
        private readonly ObtenerProximaEntregaHandler _handler;

        public ObtenerProximaEntregaHandlerTests()
        {
            _repositoryMock = new Mock<ICalendarioEntregaRepository>();
            _handler = new ObtenerProximaEntregaHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_DebeRetornarProximaEntrega()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var fecha1 = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var fecha2 = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

            calendar.AgregarDireccion(fecha1, "Av. Test 123", "Ref1", new Latitud(-12.0), new Longitud(-77.0));
            calendar.AgregarDireccion(fecha2, "Av. Test 456", "Ref2", new Latitud(-12.1), new Longitud(-77.1));

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(
                new ObtenerProximaEntregaQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Fecha.Should().Be(fecha1);
        }

        [Fact]
        public async Task Handle_DebeRetornarNullSiNoHayProximaEntrega()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(
                new ObtenerProximaEntregaQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_DebeRetornarNullSiProximaEntregaEstaInactiva()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            calendar.AgregarDireccion(fecha, "Av. Test 123", "Ref1", new Latitud(-12.0), new Longitud(-77.0));
            
            var direccion = calendar.ObtenerDireccion(fecha);
            direccion!.MarcarNoEntrega();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(
                new ObtenerProximaEntregaQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiCalendarioNoExiste()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync((CalendarioEntrega?)null);

            // Act
            var result = await _handler.Handle(
                new ObtenerProximaEntregaQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("ItemNotFound");
        }
    }
}
