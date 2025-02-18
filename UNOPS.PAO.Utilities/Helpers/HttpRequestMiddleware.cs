namespace UNOPS.PAO.Utilities.Helpers;

using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;

public class HttpRequestMiddleware
{
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly RequestDelegate next;
    private readonly IServiceProvider serviceProvider;

    public HttpRequestMiddleware(RequestDelegate next, IServiceProvider serviceProvider,
        IWebHostEnvironment hostEnvironment)
    {
        this.next = next;
        this.serviceProvider = serviceProvider;
        this.hostEnvironment = hostEnvironment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.Request.IsHttps || context.Request.Headers["X-Forwarded-Proto"] == Uri.UriSchemeHttps)
            {
                await next(context);
            }
            else
            {
                var queryString = context.Request.QueryString.HasValue
                    ? context.Request.QueryString.Value
                    : string.Empty;
                var https = "https://" + context.Request.Host + context.Request.Path + queryString;
                context.Response.Redirect(https);
            }
        }
        catch (Exception e)
        {
            if (!hostEnvironment.IsDevelopment() && e is not BusinessException)
            {
                using var scope = serviceProvider.CreateScope();
                var baseDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var url = context.Request.GetDisplayUrl();

                var errorLog = new ErrorLog(e.Message, e.StackTrace, context.Response.StatusCode, url, context.User?.Identity?.Name);
                //baseDbContext.ErrorLogs.Add(errorLog);
                await baseDbContext.SaveChangesAsync();

                // var emailSenderCoordinator = scope.ServiceProvider.GetRequiredService<IEmailSenderCoordinator>();
                //  emailSenderCoordinator.SendErrorLog(errorLog);
            }

            var code = 500;

            if (e is ApplicationException || e is BusinessException)
            {
                code = 400;
            }
            else if (e is UnauthorizedAccessException)
            {
                code = 403;
            }
            else if (e is AuthenticationException)
            {
                code = 401;
            }
            else if (e is KeyNotFoundException)
            {
                code = 404;
            }

            var message = code == 500 ? "Server error occurred" : e.Message;

            context.Response.StatusCode = code;

            var problemDetails = new ProblemDetails
            {
                Status = code,
                Title = message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            };

            if (hostEnvironment.IsDevelopment())
            {
                problemDetails.Extensions = new Dictionary<string, object?>()
                {
                    { "StackTrace", e.StackTrace }
                };
            }

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

public static class HttpRequestMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpRequestMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpRequestMiddleware>();
    }
}