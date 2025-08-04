namespace Shared.Constants
{
    public static class ExceptionMessages
    {
        public const string DefaultValidationError = "One or more validation errors have occurred";
        public const string DefaultWorkflowError = "One or more errors have occurred";
        public const string UnexpectedError = "Something went wrong";
        public const string ConcurrencyConflict = "Data has been changed by someone else since you opened it. You will need to refresh it and discard your changes";
    }
}
