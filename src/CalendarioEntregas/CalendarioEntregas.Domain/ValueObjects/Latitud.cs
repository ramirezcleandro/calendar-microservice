namespace CalendarioEntregas.Domain.ValueObjects
{
    public record Latitud
    {
        public double Valor { get; init; }

        public Latitud(double valor)
        {
            ValidarLatitud(valor);
            Valor = valor;
        }

        private static void ValidarLatitud(double valor)
        {
            if (valor < -90 || valor > 90)
            {
                throw new ArgumentException("La latitud debe estar entre -90 y 90 grados");
            }
        }
    }
}
