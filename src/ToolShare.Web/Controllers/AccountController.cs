using Microsoft.AspNetCore.Mvc; using ToolShare.Web.Services;
namespace ToolShare.Web.Controllers;
public class AccountController(ApiClient api) : Controller {
  public IActionResult Login() => View();
  [HttpPost] public async Task<IActionResult> Login(string email, string password) => await SignIn(email, password) ?? Fail("We could not sign you in. Check your email and password, then try again.", email);
  public IActionResult Register() => View();
  [HttpPost] public async Task<IActionResult> Register(string name, string email, string password) {
    var (ok, msg) = await api.Do(HttpMethod.Post, "api/auth/register", new { name, email, password });
    if (!ok) { ViewBag.Name = name; return Fail(msg, email); }
    return await SignIn(email, password) ?? RedirectToAction("Login");
  }
  [HttpPost] public async Task<IActionResult> Logout() { await api.Do(HttpMethod.Post, "api/auth/logout"); HttpContext.Session.Clear(); return RedirectToAction("Login"); }
  async Task<IActionResult?> SignIn(string email, string password) {
    var (ok, role, id) = await api.Login(email, password); if (!ok) return null;
    HttpContext.Session.SetInt32("UserId", id); HttpContext.Session.SetString("Role", role!);
    return RedirectToAction("Index", "Tools");
  }
  IActionResult Fail(string msg, string email) { ViewBag.Error = msg; ViewBag.Email = email; return View(); }
}
