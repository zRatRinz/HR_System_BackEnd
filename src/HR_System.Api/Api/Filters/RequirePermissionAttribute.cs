using HR_System.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HR_System.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    public string[] Permissions { get; }

    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                status = "fail",
                message = "Invalid or expired token",
                data = (object?)null
            });
            return;
        }

        var currentUserService = context.HttpContext.RequestServices.GetService(typeof(ICurrentUserService)) as ICurrentUserService;
        if (currentUserService == null)
        {
            context.Result = new ObjectResult(new
            {
                status = "fail",
                message = "Service unavailable",
                data = (object?)null
            })
            {
                StatusCode = 503
            };
            return;
        }

        var hasPermission = Permissions.Any(p => currentUserService.HasPermission(p));
        if (!hasPermission)
        {
            context.Result = new ObjectResult(new
            {
                status = "fail",
                message = "Insufficient permissions",
                data = (object?)null
            })
            {
                StatusCode = 403
            };
        }
    }
}