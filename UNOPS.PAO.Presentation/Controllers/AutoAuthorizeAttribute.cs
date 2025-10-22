using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation
{
    /// <summary>
    /// Filter attribute to automatically check permissions based on controller conventions
    /// </summary>
    public class AutoAuthorizeAttribute : TypeFilterAttribute
    {
        public AutoAuthorizeAttribute() : base(typeof(AutoAuthorizeFilter))
        {
        }
        
        private class AutoAuthorizeFilter : IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.Controller is BaseController controller)
                {
                    if (await controller.AutoAuthorizeRequest(context))
                    {
                        // Authorized, continue with the action
                        await next();
                    }
                    // Otherwise, result has already been set in the context
                }
                else
                {
                    // Not a BaseController, just continue
                    await next();
                }
            }
        }
    }
} 