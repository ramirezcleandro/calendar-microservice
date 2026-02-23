using CalendarioEntregas.Application.Calendario.AgregarDireccion;
using CalendarioEntregas.Application.Calendario.CreateCalendario;
using CalendarioEntregas.Application.Calendario.GetCalendario;
using CalendarioEntregas.Application.Calendario.ListarCalendarios;
using CalendarioEntregas.Application.Calendario.MarcarDiaNoEntrega;
using CalendarioEntregas.Application.Calendario.ModificarDireccion;
using CalendarioEntregas.Application.Calendario.ReactivarEntrega;
using CalendarioEntregas.Application.Calendario.ObtenerProximaEntrega;
using CalendarioEntregas.Application.Calendario.ObtenerDireccionesActivas;
using CalendarioEntregas.Application.Calendario.DesactivarCalendario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CalendarioEntregas.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarioController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CalendarioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("crear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearCalendario([FromBody] CreateCalendarioCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(new { calendarioId = result.Value });
        }

        [HttpGet("{calendarioId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerCalendario(Guid calendarioId)
        {
            var result = await _mediator.Send(new GetCalendarioQuery(calendarioId));

            if (result.IsFailure)
            {
                return NotFound(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCalendarios([FromQuery] Guid? pacienteId = null)
        {
            var result = await _mediator.Send(new ListarCalendariosQuery(pacienteId));

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(result.Value);
        }

        [HttpPost("{calendarioId:guid}/agregar-direccion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AgregarDireccion(
            Guid calendarioId,
            [FromBody] AgregarDireccionCommand command)
        {
            var cmd = command with { CalendarioId = calendarioId };
            var result = await _mediator.Send(cmd);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(new { direccionId = result.Value });
        }

        [HttpPut("{calendarioId:guid}/modificar-direccion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ModificarDireccion(
            Guid calendarioId,
            [FromBody] ModificarDireccionCommand command)
        {
            var cmd = command with { CalendarioId = calendarioId };
            var result = await _mediator.Send(cmd);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(new { mensaje = "Dirección modificada exitosamente" });
        }

        [HttpPost("{calendarioId:guid}/marcar-no-entrega")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarcarNoEntrega(
            Guid calendarioId,
            [FromBody] MarcarDiaNoEntregaCommand command)
        {
            var cmd = command with { CalendarioId = calendarioId };
            var result = await _mediator.Send(cmd);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(new { mensaje = "Entrega marcada como no activa" });
        }

        [HttpPost("{calendarioId:guid}/reactivar-entrega")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivarEntrega(
            Guid calendarioId,
            [FromBody] ReactivarEntregaCommand command)
        {
            var cmd = command with { CalendarioId = calendarioId };
            var result = await _mediator.Send(cmd);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(new { mensaje = "Entrega reactivada exitosamente" });
        }

        [HttpGet("{calendarioId:guid}/proxima-entrega")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerProximaEntrega(Guid calendarioId)
        {
            var result = await _mediator.Send(new ObtenerProximaEntregaQuery(calendarioId));

            if (result.IsFailure)
            {
                return NotFound(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(result.Value);
        }

        [HttpGet("{calendarioId:guid}/direcciones-activas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerDireccionesActivas(Guid calendarioId)
        {
            var result = await _mediator.Send(new ObtenerDireccionesActivasQuery(calendarioId));

            if (result.IsFailure)
            {
                return NotFound(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(result.Value);
        }

        [HttpPost("{calendarioId:guid}/desactivar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DesactivarCalendario(Guid calendarioId)
        {
            var result = await _mediator.Send(new DesactivarCalendarioCommand(calendarioId));

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    error = result.Error.Code,
                    mensaje = result.Error.Description
                });
            }

            return Ok(new { mensaje = "Calendario desactivado exitosamente" });
        }
    }
}
