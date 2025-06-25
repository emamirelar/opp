using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace UNOPS.PAO.Server.Middleware;

/// <summary>
/// Middleware to handle model validation errors consistently across the API
/// </summary>
public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}

/// <summary>
/// Custom validation problem details for better error responses
/// </summary>
public class CustomValidationProblemDetails : ValidationProblemDetails
{
    public CustomValidationProblemDetails(ModelStateDictionary modelState)
        : base(modelState)
    {
        Title = "Validation failed";
        Detail = "One or more validation errors occurred";
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        Status = 400;
    }

    public CustomValidationProblemDetails(string field, string error)
    {
        Title = "Validation failed";
        Detail = "One or more validation errors occurred";
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        Status = 400;
        Errors[field] = new[] { error };
    }

    public CustomValidationProblemDetails(Dictionary<string, string[]> errors)
    {
        Title = "Validation failed";
        Detail = "One or more validation errors occurred";
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        Status = 400;
        Errors = errors;
    }
}