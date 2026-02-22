using CalendarioEntregas.Application.Calendario.ModificarDireccion;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
    public class ModificarDireccionHandlerTests
    {
        private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ModificarDireccionHandler _handler;

        public ModificarDireccionHandlerTests()
        {
            _repositoryMock = new Mock<ICalendarioEntregaRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new ModificarDireccionHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_DeberiaModificarDireccionConDosdiasAnticipacion()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var fechaEntrega = DateOnly.FromDateTime(DateTime.Today.AddDays(5)); // 5 días de anticipación
            calendar.AgregarDireccion(
                fechaEntrega,
                "Av. Test 123",
                "Referencias",
                new Latitud(-12.0),
                new Longitud(-77.0)
            );

            var command = new ModificarDireccionCommand(
                calendarioId,
                fechaEntrega,
                "Av. Nueva 456",
                "Referencias Nuevas",
                -12.1,
                -77.1
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<CalendarioEntrega>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _repositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<CalendarioEntrega>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiNoHayDosdiasDeAnticipacion()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var fechaEntrega = DateOnly.FromDateTime(DateTime.Today.AddDays(1)); // Solo 1 día
            calendar.AgregarDireccion(
                fechaEntrega,
                "Av. Test 123",
                "Referencias",
                new Latitud(-12.0),
                new Longitud(-77.0)
            );

            var command = new ModificarDireccionCommand(
                calendarioId,
                fechaEntrega,
                "Av. Nueva 456",
                "Referencias Nuevas",
                -12.1,
                -77.1
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Calendario.ModificacionError");
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiCalendarioNoExiste()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var command = new ModificarDireccionCommand(
                calendarioId,
                new DateOnly(2025, 2, 5),
                "Av. Nueva 456",
                "Referencias Nuevas",
                -12.1,
                -77.1
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync((CalendarioEntrega?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("ItemNotFound");
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiDireccionNoExiste()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var command = new ModificarDireccionCommand(
                calendarioId,
                new DateOnly(2025, 2, 5),
                "Av. Nueva 456",
                "Referencias Nuevas",
                -12.1,
                -77.1
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("ItemNotFound");
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiLatitudEsInvalida()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var fechaEntrega = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            calendar.AgregarDireccion(
                fechaEntrega,
                "Av. Test 123",
                "Referencias",
                new Latitud(-12.0),
                new Longitud(-77.0)
            );

            var command = new ModificarDireccionCommand(
                calendarioId,
                fechaEntrega,
                "Av. Nueva 456",
                "Referencias Nuevas",
                -91.0, // Latitud inválida
                -77.1
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Calendario.ValidacionError");
        }
    }
}
