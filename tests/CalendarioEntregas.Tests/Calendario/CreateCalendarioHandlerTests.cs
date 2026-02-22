using CalendarioEntregas.Application.Calendario.CreateCalendario;
using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
    public class CreateCalendarioHandlerTests
    {
        private readonly Mock<ICalendarioEntregaRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateCalendarioHandler _handler;

        public CreateCalendarioHandlerTests()
        {
            _repositoryMock = new Mock<ICalendarioEntregaRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new CreateCalendarioHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_DeberiaCrearCalendarioCorrectamente()
        {
            // Arrange
            var command = new CreateCalendarioCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2025, 2, 1),
                new DateOnly(2025, 2, 15)
            );

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Domain.Agregados.CalendarioEntrega>()))
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
                r => r.AddAsync(It.IsAny<Domain.Agregados.CalendarioEntrega>()),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                u => u.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_DeberiaFallarSiFechaFinAnteriorAFechaInicio()
        {
            // Arrange
            var command = new CreateCalendarioCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2025, 2, 15),
                new DateOnly(2025, 2, 1) // Fecha fin antes que inicio
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Calendario.CreacionError");
        }
    }
}
