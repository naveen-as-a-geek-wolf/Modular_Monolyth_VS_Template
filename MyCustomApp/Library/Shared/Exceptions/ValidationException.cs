using FluentValidation.Results;
using Shared.Constants;

namespace Shared.Exceptions
{
    public class ValidationException : WorkflowException
    {
        public ValidationException()
            : base(ExceptionMessages.DefaultValidationError)
        {
            base.Errors = new Dictionary<string, string>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            SetErrors(failures);
        }

        public ValidationException(IEnumerable<ErrorResponse> failures)
            : this()
        {
            base.Errors = failures.ToDictionary((ErrorResponse failure) => failure.ErrorCode, (ErrorResponse failure) => failure.Message);
        }

        public ValidationException(IEnumerable<ErrorResponse> failures, object? errorData)
            : this()
        {
            base.Errors = failures.ToDictionary((ErrorResponse failure) => failure.ErrorCode, (ErrorResponse failure) => failure.Message);
            base.ErrorData = errorData;
        }

        public ValidationException(string errorCode, string failure)
            : this()
        {
            base.Errors = new Dictionary<string, string> { { errorCode, failure } };
        }

        private void SetErrors(IEnumerable<ValidationFailure> failures)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (ValidationFailure failure in failures)
            {
                string errorCode = failure.ErrorCode;
                if (dictionary.TryGetValue(failure.ErrorCode, out var _))
                {
                    Random random = new();
                    errorCode = GetErrorCode(dictionary, errorCode, random);
                }

                dictionary.Add(errorCode, failure.ErrorMessage);
            }

            base.Errors = dictionary;
        }

        private static string GetErrorCode(IDictionary<string, string> errors, string errorCode, Random random)
        {
            errorCode += random.Next(100);
            if (errors.TryGetValue(errorCode, out string _))
            {
                return GetErrorCode(errors, errorCode, random);
            }

            return errorCode;
        }
    }
}
