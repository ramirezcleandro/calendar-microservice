using CalendarioEntregas.Application.Calendario.AgregarDireccion;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
    public class AgregarDireccionHandlerTests
    {
        private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AgregarDireccionHandler _handler;

        public AgregarDireccionHandlerTests()
        {
            _repositoryMock = new Mock<ICalendarioEntregaRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new AgregarDireccionHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_DeberiaAgregarDireccionCorrectamente()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendario = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2025, 2, 1),
                new DateOnly(2025, 2, 15)
            );

            var command = new AgregarDireccionCommand(
                calendarioId,
                new DateOnly(2025, 2, 5),
                "Av. Principal 123",
                "Frente al parque",
                -12.0464,
                -77.0428
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendario);

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
            result.Value.Should().NotBeEmpty();

            _repositoryMock.Verify(
                r => r.GetByIdAsync(calendarioId),
                Times.Once
            );

            _repositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<CalendarioEntrega>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiCalendarioNoExiste()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();

            var command = new AgregarDireccionCommand(
                calendarioId,
                new DateOnly(2025, 2, 5),
                "Av. Principal 123",
                "Frente al parque",
                -12.0464,
                -77.0428
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
        public async Task Handle_DeberiaFallarSiLatitudEsInvalida()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendario = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2025, 2, 1),
                new DateOnly(2025, 2, 15)
            );

            var command = new AgregarDireccionCommand(
                calendarioId,
                new DateOnly(2025, 2, 5),
                "Av. Principal 123",
                "Frente al parque",
                -91, // Latitud inválida
                -77.0428
            );

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendario);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
