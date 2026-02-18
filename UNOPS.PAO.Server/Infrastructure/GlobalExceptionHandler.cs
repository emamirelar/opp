namespace UNOPS.PAO.Server.Infrastructure;
    
using System.Security.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Domain.Infrastructure;


public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly IServiceProvider serviceProvider;

    public GlobalExceptionHandler(IWebHostEnvironment hostEnvironment, IServiceProvider serviceProvider)
    {
        this.hostEnvironment = hostEnvironment;
        this.serviceProvider = serviceProvider;
    }

    private ProblemDetails GetDetails(Exception exception)
    {
        if (exception is ApplicationException || exception is BusinessException)
        {
            return new ProblemDetails
            {
                Status = 400,
                Title = exception.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            };
        }

        if (exception is UnauthorizedAccessException)
        {
            return new ProblemDetails
            {
                Status = 403,
                Title = exception.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"
            };
        }

        if (exception is AuthenticationException)
        {
            return new ProblemDetails
            {
                Status = 401,
                Title = exception.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1"
            };
        }

        if (exception is KeyNotFoundException)
        {
            return new ProblemDetails
            {
                Status = 404,
                Title = exception.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
            };
        }

        return new ProblemDetails
        {
            Status = 500,
            Title = "Server error occurred",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // TODO: Add ErrorLog to database or use ILogger
        //if (!hostEnvironment.IsDevelopment() && exception is not BusinessException)
        //{
        //    using var scope = serviceProvider.CreateScope();
        //    var baseDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //    var url = httpContext.Request.GetDisplayUrl();

        //    var errorLog = new ErrorLog(exception.Message, exception.StackTrace, httpContext.Response.StatusCode, url, httpContext.User?.Identity?.Name);
        //    //baseDbContext.ErrorLogs.Add(errorLog);
        //    await baseDbContext.SaveChangesAsync();
        //}

        var problemDetails = GetDetails(exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;

        if (hostEnvironment.IsDevelopment())
        {
            problemDetails.Extensions = new Dictionary<string, object?>()
                {
                    { "StackTrace", exception.StackTrace }
                };
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails);

        return true;
    }
}
