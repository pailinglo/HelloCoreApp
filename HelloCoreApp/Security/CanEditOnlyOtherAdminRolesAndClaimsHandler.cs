using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelloCoreApp.Security
{
    //One-to-one relation: one requirement is handled by one handler, then you can specify the requirement type to fulfill.
    //public class PermissionHandler : IAuthorizationHandler <-- multiple requirement to fulfill in one handler.
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : 
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement)
        {
            //Resource property of AuthorizationHandlerContext returns the resource that we are protecting.
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            
            //If AuthorizationFilterContext is NULL, we cannot check if the requirement is met or not, 
            //so we return Task.CompletedTask and the access is not authorised.
            if (authFilterContext == null)
            {
                return Task.CompletedTask;
            }

            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            //only if this Admin user is not editing itself that it can proceed.
            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") &&
                adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                //using context.Succeed to indicate this requirement passes the check.
                context.Succeed(requirement);

                //generally, we don't handle failure since other handlers for the same requirement may succeed.
                //the reason for multiple hanlders for the same requirement: allow multiple different ways to pass the requirement.
            }

            return Task.CompletedTask;
        }
    }
}
