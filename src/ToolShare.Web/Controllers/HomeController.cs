using Microsoft.AspNetCore.Mvc;
namespace ToolShare.Web.Controllers;
public class HomeController : Controller {
  public IActionResult Index() => RedirectToAction("Index", "Tools");
  public IActionResult Accessibility() => View();
}
