using Shared.Constants;

namespace Shared.Exceptions
{
    public class WorkflowException : Exception
    {
        public IDictionary<string, string> Errors { get; protected set; }

        public object? ErrorData { get; set; }

        public WorkflowException()
            : base(ExceptionMessages.DefaultWorkflowError)
        {
            Errors = new Dictionary<string, string>();
        }

        public WorkflowException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string>();
        }

        public WorkflowException(ErrorResponse error)
            : this()
        {
            Errors = new Dictionary<string, string> { { error.ErrorCode, error.Message } };
        }

        public WorkflowException(IEnumerable<ErrorResponse> errors, object? data = null)
            : this()
        {
            Errors = new Dictionary<string, string>();
            foreach (ErrorResponse error in errors)
            {
                Errors.TryAdd(error.ErrorCode, error.Message);
            }

            ErrorData = data;
        }

        public ProblemDetails ToProblemDetails()
        {
            return new ProblemDetails(Errors, ErrorData);
        }
    }
}
