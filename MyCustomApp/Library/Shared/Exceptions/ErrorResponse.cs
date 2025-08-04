namespace Shared.Exceptions
{
    public class ErrorResponse
    {
        public string ErrorCode { get; protected set; }

        public string Message { get; protected set; }

        public ErrorResponse(int errorCode, string message)
        {
            ErrorCode = errorCode.ToString();
            Message = message;
        }

        public ErrorResponse(string errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}
