using LanguageExt.Common;
using Shared.Exceptions;

namespace MyCustomApp.API.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToResponse<TResult, TContract>(
                this Result<TResult> result,
                Func<TResult, TContract> mapper)
        {
            var re = result.Match<IResult>(
                success => Results.Ok(mapper(success)),
                failure =>
                {
                    return failure switch
                    {
                        ValidationException validationEx => Results.UnprocessableEntity(validationEx.ToProblemDetails()),
                        WorkflowException workflowEx => Results.BadRequest(workflowEx.ToProblemDetails()),
                        _ => Results.StatusCode(500)
                    };
                });
            return re;
        }
    }
}
