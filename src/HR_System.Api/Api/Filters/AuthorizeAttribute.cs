using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HR_System.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string[] Roles { get; }

    public AuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                status = "fail",
                message = "Invalid or expired token",
                data = (object)null
            });
            return;
        }

        if (Roles.Length > 0)
        {
            var hasRole = Roles.Any(role =>
                user.IsInRole(role) ||
                user.IsInRole(role.ToLower()) ||
                user.IsInRole(role.ToUpper()));

            if (!hasRole)
            {
                context.Result = new ObjectResult(new
                {
                    status = "fail",
                    message = "Insufficient permissions",
                    data = (object)null
                })
                {
                    StatusCode = 403
                };
            }
        }
    }
}
