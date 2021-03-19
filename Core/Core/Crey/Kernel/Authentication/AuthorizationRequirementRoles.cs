using Crey.PushNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crey.Kernel.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireRoleAttribute : Attribute
    {
        public RequireRoleAttribute(string roles)
        {
            RequiredRoles = roles;
        }

        public string RequiredRoles { get; }
    }

    public class RoleAuthorizationRequirement : IAuthorizationRequirement
    {
    }

    public class RoleAuthorizationRequirementHandler : AuthorizationHandler<RoleAuthorizationRequirement>
    {
        private readonly ILogger<RoleAuthorizationRequirementHandler> logger_;

        public RoleAuthorizationRequirementHandler(ILogger<RoleAuthorizationRequirementHandler> logger)
        {
            logger_ = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleAuthorizationRequirement requirement)
        {
            if (context.User == null)
            {
                logger_.LogInformation("missing user principal");
                context.Fail();
                return Task.CompletedTask;
            }

            var creyAuthorizeData = new List<RequireRoleAttribute>();
            var controllerMetadata =
                (context.Resource as RouteEndpoint)?.Metadata
                ??
                (context.Resource as AuthorizationFilterContext)?.HttpContext.GetEndpoint()?.Metadata; // legacy, remove upon full 3.1 migration
            if (controllerMetadata != null)
            {
                creyAuthorizeData.AddRange(controllerMetadata.GetOrderedMetadata<RequireRoleAttribute>() ?? Array.Empty<RequireRoleAttribute>());
            }
            else
            {
                // SignalR Hub

                var attributes = typeof(CreyHub<>).GetCustomAttributes(typeof(RequireRoleAttribute), inherit: true);
                foreach (var attribute in attributes)
                {
                    creyAuthorizeData.Add(attribute as RequireRoleAttribute);
                }
            }

            foreach (var creyAuthorizeDatum in creyAuthorizeData)
            {
                var rolesSplit = creyAuthorizeDatum.RequiredRoles?.Split(',');
                if (rolesSplit != null && rolesSplit.Any(role => !context.User.IsInRole(role.Trim())))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public static class RoleAuthorizationRequirementExtensions
    {
        public static AuthorizationPolicyBuilder RequiresUserRolesFromAttributes(this AuthorizationPolicyBuilder builder)
        {
            return builder.AddRequirements(new RoleAuthorizationRequirement());
        }
    }
}
