using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Filters;
namespace ToolShare.Web.Filters;
public class RequireRoleAttribute(string? role = null) : ActionFilterAttribute {
  public override void OnActionExecuting(ActionExecutingContext c) {
    var r = c.HttpContext.Session.GetString("Role");
    if (r == null) c.Result = new RedirectToActionResult("Login", "Account", null);
    else if (role != null && r != role) c.Result = new RedirectToActionResult("Index", "Tools", null);
  }
}
