using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Presentation.Filters;

/// <summary>
/// Action filter that automatically validates model state and returns ValidationProblemDetails if invalid
/// </summary>
public class ValidateModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
            {
                Title = "One or more validation errors occurred",
                Detail = "Please check the errors and try again"
            });
        }
    }
}

/// <summary>
/// Action filter that validates specific parameter types and provides detailed error messages
/// </summary>
public class ValidateParametersAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var errors = new Dictionary<string, string[]>();

        // Check for common validation issues
        foreach (var param in context.ActionArguments)
        {
            var paramName = param.Key;
            var paramValue = param.Value;

            // Validate pagination requests
            if (paramValue is PaginationRequest paginationRequest)
            {
                if (paginationRequest.PageIndex < 1)
                {
                    errors[nameof(paginationRequest.PageIndex)] = new[] { "Page index must be greater than 0" };
                }

                if (paginationRequest.PageSize < 1)
                {
                    errors[nameof(paginationRequest.PageSize)] = new[] { "Page size must be greater than 0" };
                }
                else if (paginationRequest.PageSize > 2000)
                {
                    errors[nameof(paginationRequest.PageSize)] = new[] { "Page size cannot exceed 2000" };
                }
            }

            // Validate required IDs
            if (paramName.EndsWith("Id") && paramValue is int id && id <= 0)
            {
                errors[paramName] = new[] { $"{paramName} must be a positive number" };
            }
        }

        if (errors.Any())
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Title = "Invalid parameters",
                Detail = "One or more parameters are invalid"
            });
        }
    }
}