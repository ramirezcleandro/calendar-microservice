namespace CalendarioEntregas.Domain.Abstractions
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; set; }
        public Error? Error { get; set; }

        private Result(bool isSuccess, T? value, Error? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(Error error)
        {
            return new Result<T>(false, default, error);
        }
    }

    public class Result
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; set; }

        private Result(bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(Error error)
        {
            return new Result(false, error);
        }
    }
}
