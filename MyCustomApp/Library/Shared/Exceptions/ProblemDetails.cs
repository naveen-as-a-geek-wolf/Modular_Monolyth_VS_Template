namespace Shared.Exceptions
{
    public class ProblemDetails(IDictionary<string, string> errors, object? errorData = null)
    {
        public IDictionary<string, string> Errors { get; } = errors;

        public object? ErrorData { get; set; } = errorData;
    }
}
