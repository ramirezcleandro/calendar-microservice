namespace CalendarioEntregas.Domain.Abstractions
{
    public class Error
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public Error(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public static Error Problem(string code, string description)
        {
            return new Error(code, description);
        }

        public static Error ItemNotFound(string description)
        {
            return new Error("ItemNotFound", description);
        }
    }
}
