using Microsoft.AspNetCore.Mvc; using ToolShare.Web.Filters; using ToolShare.Web.Models; using ToolShare.Web.Services;
namespace ToolShare.Web.Controllers;
[RequireRole]
public class ToolsController(ApiClient api) : Controller {
  public async Task<IActionResult> Index(string? search, bool? available) {
    var q = new List<string>();
    if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + Uri.EscapeDataString(search));
    if (available == true) q.Add("available=true");
    ViewBag.Search = search; ViewBag.OnlyAvailable = available == true;
    var tools = await api.Get<List<ToolVm>>("api/tools" + (q.Count > 0 ? "?" + string.Join("&", q) : ""));
    if (tools == null) ViewBag.Error = "We could not load the tools. Please try again in a moment.";
    return View(tools ?? new());
  }
  [HttpPost] public async Task<IActionResult> Borrow(int id, int days, string name) {
    var (ok, msg) = await api.Do(HttpMethod.Post, $"api/tools/{id}/borrow", new { days });
    TempData[ok ? "Ok" : "Error"] = ok ? $"You borrowed {name}. Please return it within {days} {(days == 1 ? "day" : "days")}." : msg;
    return ok ? RedirectToAction("Mine", "Loans") : RedirectToAction("Index");
  }
}
