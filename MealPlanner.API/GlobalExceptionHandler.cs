using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private record Error(HttpStatusCode StatusCode, string Message)
    {
        public static Error InternalServerError => new(HttpStatusCode.InternalServerError, "Internal server error");
        public static Error BadRequest(string message) => new(HttpStatusCode.BadRequest, message);
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var error = exception switch
        {
            ArgumentNullException ex => Error.BadRequest(ex.Message),
            ArgumentOutOfRangeException ex => Error.BadRequest(ex.Message),
            InvalidOperationException ex => Error.BadRequest(ex.Message), 
            _ => Error.InternalServerError
        };
        
        var statusCode = (int)error.StatusCode;
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Type = exception.GetType().Name,
            Title = "An error occured",
            Detail = error.Message,
            Status = statusCode
        };
        
        
        var result = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails 
        });

        return result;
    }
}