namespace Shared.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string ErrorMessage { get; protected set; } = string.Empty;
        public List<string> Errors { get; protected set; } = new();

        protected Result() { }

        public static Result Success() => new() { IsSuccess = true };

        public static Result Failure(string error) => new()
        {
            IsSuccess = false,
            ErrorMessage = error
        };

        public static Result Failure(string error, List<string> errors) => new()
        {
            IsSuccess = false,
            ErrorMessage = error,
            Errors = errors
        };
    }

    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        private Result() { }

        public static Result<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data
        };

        public static new Result<T> Failure(string error) => new()
        {
            IsSuccess = false,
            ErrorMessage = error
        };

        public static new Result<T> Failure(string error, List<string> errors) => new()
        {
            IsSuccess = false,
            ErrorMessage = error,
            Errors = errors
        };
    }
}
