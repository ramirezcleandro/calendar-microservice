using CalendarioEntregas.Application.Calendario.ObtenerDireccionesActivas;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
    public class ObtenerDireccionesActivasHandlerTests
    {
        private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
        private readonly ObtenerDireccionesActivasHandler _handler;

        public ObtenerDireccionesActivasHandlerTests()
        {
            _repositoryMock = new Mock<ICalendarioEntregaRepository>();
            _handler = new ObtenerDireccionesActivasHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_DebeRetornarTodasLasDireccionesActivas()
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
            var fecha3 = DateOnly.FromDateTime(DateTime.Today.AddDays(15));

            calendar.AgregarDireccion(fecha1, "Av. Test 123", "Ref1", new Latitud(-12.0), new Longitud(-77.0));
            calendar.AgregarDireccion(fecha2, "Av. Test 456", "Ref2", new Latitud(-12.1), new Longitud(-77.1));
            calendar.AgregarDireccion(fecha3, "Av. Test 789", "Ref3", new Latitud(-12.2), new Longitud(-77.2));

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(
                new ObtenerDireccionesActivasQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(3);
        }

        [Fact]
        public async Task Handle_DebeExcluirDireccionesInactivas()
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

            // Marcar la primera como inactiva
            var direccion1 = calendar.ObtenerDireccion(fecha1);
            direccion1!.MarcarNoEntrega();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(
                new ObtenerDireccionesActivasQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value!.First().Fecha.Should().Be(fecha2);
        }

        [Fact]
        public async Task Handle_DebeRetornarListaVaciaParaCalendarioSinDirecciones()
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
                new ObtenerDireccionesActivasQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
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
                new ObtenerDireccionesActivasQuery(calendarioId),
                CancellationToken.None
            );

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("ItemNotFound");
        }
    }
}
