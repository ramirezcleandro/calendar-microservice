using CalendarioEntregas.Application.Calendario.DesactivarCalendario;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.ValueObjects;
using CalendarioEntregas.Domain.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
    public class DesactivarCalendarioHandlerTests
    {
        private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DesactivarCalendarioHandler _handler;

        public DesactivarCalendarioHandlerTests()
        {
            _repositoryMock = new Mock<ICalendarioEntregaRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new DesactivarCalendarioHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_DeberiaDesactivarCalendarioActivo()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var command = new DesactivarCalendarioCommand(calendarioId);

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
            calendar.Activo.Should().BeFalse();

            _repositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<CalendarioEntrega>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiCalendarioYaEstaDesactivado()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var calendar = new CalendarioEntrega(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            // Desactivar primero
            calendar.Desactivar();

            var command = new DesactivarCalendarioCommand(calendarioId);

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync(calendar);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Calendario.YaDesactivado");
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiCalendarioNoExiste()
        {
            // Arrange
            var calendarioId = Guid.NewGuid();
            var command = new DesactivarCalendarioCommand(calendarioId);

            _repositoryMock
                .Setup(r => r.GetByIdAsync(calendarioId))
                .ReturnsAsync((CalendarioEntrega?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("ItemNotFound");
        }
    }
}
