using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Filters;
namespace ToolShare.Api.Security;
/// <summary>Requires a logged-in session, optionally limited to the given roles. 401 = not logged in, 403 = wrong role.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthAttribute(params string[] roles) : Attribute, IAuthorizationFilter {
  public void OnAuthorization(AuthorizationFilterContext c) {
    if (c.HttpContext.Session.GetInt32("UserId") == null) { c.Result = new UnauthorizedResult(); return; }
    if (roles.Length > 0 && !roles.Contains(c.HttpContext.Session.GetString("Role"))) c.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
  }
}
