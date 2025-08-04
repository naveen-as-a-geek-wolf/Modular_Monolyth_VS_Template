using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Exceptions;
using System.Data;
using System.Net;
using System.Text.Json;

namespace Shared.Web.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                HttpResponse response = context.Response;
                response.ContentType = "application/json";
                string result = string.Empty;
                JsonSerializerOptions options = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                switch (error)
                {
                    case TaskCanceledException:
                        break;
                    case UnauthorizedAccessException e:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        result = e.Message;
                        if (!result.Contains(ExceptionConstants.TokenExpired))
                        {
                            _logger.LogError(e, message: e.Message);
                        }
                        break;
                    case ValidationException e:
                        response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                        result = JsonSerializer.Serialize(new { e.Errors }, options);
                        break;

                    case WorkflowException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        result = JsonSerializer.Serialize(new { e.Errors }, options);
                        break;

                    case DbUpdateConcurrencyException e:
                        result = GetConcurrencyError(response, options, e);
                        break;

                    case DBConcurrencyException y:
                        result = GetConcurrencyError(response, options, y);
                        break;

                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonSerializer.Serialize(new
                        {
                            Errors = new Dictionary<int, string>
                        { { 0, ExceptionMessages.UnexpectedError } }
                        }, options);
                        _logger.LogError(error, message: error.Message);
                        break;
                }
                if (!string.IsNullOrEmpty(result))
                {
                    await response.WriteAsync(result);
                }
            }
        }

        private string GetConcurrencyError(HttpResponse response, JsonSerializerOptions options, Exception e)
        {
            string result;
            response.StatusCode = (int)HttpStatusCode.Conflict;
            result = JsonSerializer.Serialize(new { Errors = new Dictionary<int, string> { { 10005, ExceptionMessages.ConcurrencyConflict } } }, options);
            _logger.LogError(e, message: e.Message);
            return result;
        }
    }
}
