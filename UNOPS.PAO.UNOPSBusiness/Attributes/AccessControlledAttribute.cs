using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AccessControlledAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _entity;
        private readonly string _action;

        public AccessControlledAttribute(string entity, string action)
        {
            _entity = entity;
            _action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            var permissionService = context.HttpContext.RequestServices
                .GetRequiredService<IPermissionService>();

            if (!await permissionService.CanPerformActionAsync(_entity, _action, user))
            {
                context.Result = new ForbidResult();
            }
        }
    }
} 