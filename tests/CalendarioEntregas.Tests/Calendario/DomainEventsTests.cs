using CalendarioEntregas.Domain.Eventos;
using FluentAssertions;
using Xunit;

namespace CalendarioEntregas.Tests.Calendario
{
	public class DomainEventsTests
	{
		private static readonly Guid CalendarioId = Guid.NewGuid();
		private static readonly Guid PacienteId = Guid.NewGuid();
		private static readonly Guid DireccionId = Guid.NewGuid();
		private static readonly DateOnly Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

		[Fact]
		public void CalendarioCreado_ExponeDatosYCumpleIgualdadDeRecord()
		{
			var planId = Guid.NewGuid();
			var inicio = DateOnly.FromDateTime(DateTime.Today);
			var fin = inicio.AddDays(30);

			var a = new CalendarioCreado(CalendarioId, PacienteId, planId, inicio, fin);
			var b = new CalendarioCreado(CalendarioId, PacienteId, planId, inicio, fin);

			a.CalendarioId.Should().Be(CalendarioId);
			a.PacienteId.Should().Be(PacienteId);
			a.PlanAlimenticioId.Should().Be(planId);
			a.FechaInicio.Should().Be(inicio);
			a.FechaFin.Should().Be(fin);
			a.Should().Be(b);
			a.GetHashCode().Should().Be(b.GetHashCode());
			a.ToString().Should().Contain(CalendarioId.ToString());
		}

		[Fact]
		public void CalendarioDesactivado_ExponeDatosYCumpleIgualdadDeRecord()
		{
			var a = new CalendarioDesactivado(CalendarioId, PacienteId);
			var b = new CalendarioDesactivado(CalendarioId, PacienteId);

			a.CalendarioId.Should().Be(CalendarioId);
			a.PacienteId.Should().Be(PacienteId);
			a.Should().Be(b);
			a.ToString().Should().Contain(CalendarioId.ToString());
		}

		[Fact]
		public void DireccionAgregada_ExponeDatosYCumpleIgualdadDeRecord()
		{
			var a = new DireccionAgregada(CalendarioId, DireccionId, Fecha, "Av. Test", -12.0, -77.0);
			var b = new DireccionAgregada(CalendarioId, DireccionId, Fecha, "Av. Test", -12.0, -77.0);

			a.DireccionId.Should().Be(DireccionId);
			a.Direccion.Should().Be("Av. Test");
			a.Latitud.Should().Be(-12.0);
			a.Longitud.Should().Be(-77.0);
			a.Should().Be(b);
			a.ToString().Should().Contain("Av. Test");
		}

		[Fact]
		public void DireccionModificada_ExponeDatosYCumpleIgualdadDeRecord()
		{
			var a = new DireccionModificada(CalendarioId, DireccionId, Fecha, "Nueva", -13.0, -78.0);
			var b = new DireccionModificada(CalendarioId, DireccionId, Fecha, "Nueva", -13.0, -78.0);

			a.NuevaDireccion.Should().Be("Nueva");
			a.NuevaLatitud.Should().Be(-13.0);
			a.NuevaLongitud.Should().Be(-78.0);
			a.Should().Be(b);
			a.ToString().Should().Contain("Nueva");
		}

		[Fact]
		public void EntregaCancelada_ExponeDatosYCumpleIgualdadDeRecord()
		{
			var a = new EntregaCancelada(CalendarioId, DireccionId, Fecha);
			var b = new EntregaCancelada(CalendarioId, DireccionId, Fecha);

			a.CalendarioId.Should().Be(CalendarioId);
			a.DireccionId.Should().Be(DireccionId);
			a.Fecha.Should().Be(Fecha);
			a.Should().Be(b);
			a.ToString().Should().Contain(CalendarioId.ToString());
		}

		[Fact]
		public void EntregaReactivada_ExponeDatosYCumpleIgualdadDeRecord()
		{
			var a = new EntregaReactivada(CalendarioId, DireccionId, Fecha);
			var b = new EntregaReactivada(CalendarioId, DireccionId, Fecha);

			a.CalendarioId.Should().Be(CalendarioId);
			a.DireccionId.Should().Be(DireccionId);
			a.Fecha.Should().Be(Fecha);
			a.Should().Be(b);
			a.ToString().Should().Contain(CalendarioId.ToString());
		}
	}
}
