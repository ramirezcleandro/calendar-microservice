namespace CalendarioEntregas.Domain.ValueObjects
{
    public record Longitud
    {
        public double Valor { get; init; }

        public Longitud(double valor)
        {
            ValidarLongitud(valor);
            Valor = valor;
        }

        private static void ValidarLongitud(double valor)
        {
            if (valor < -180 || valor > 180)
            {
                throw new ArgumentException("La longitud debe estar entre -180 y 180 grados");
            }
        }
    }
}
