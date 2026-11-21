using Microsoft.AspNetCore.Mvc; using ToolShare.Web.Filters; using ToolShare.Web.Models; using ToolShare.Web.Services;
namespace ToolShare.Web.Controllers;
[RequireRole("Admin")]
public class AdminController(ApiClient api) : Controller {
  public async Task<IActionResult> Tools() {
    var tools = await api.Get<List<ToolVm>>("api/tools"); if (tools == null) ViewBag.Error = "We could not load the tools. Please try again.";
    return View(tools ?? new());
  }
  [HttpPost] public async Task<IActionResult> AddTool(string name, string description, string category) {
    var (ok, msg) = await api.Do(HttpMethod.Post, "api/tools", new { name, description, category });
    TempData[ok ? "Ok" : "Error"] = ok ? $"{name} was added to the library." : msg; return RedirectToAction("Tools");
  }
  public async Task<IActionResult> Edit(int id) { var t = await api.Get<ToolVm>($"api/tools/{id}"); return t == null ? RedirectToAction("Tools") : View(t); }
  [HttpPost] public async Task<IActionResult> Edit(int id, string name, string description, string category) {
    var (ok, msg) = await api.Do(HttpMethod.Put, $"api/tools/{id}", new { name, description, category });
    TempData[ok ? "Ok" : "Error"] = ok ? $"{name} was updated." : msg; return RedirectToAction("Tools");
  }
  public async Task<IActionResult> ConfirmDelete(int id) { var t = await api.Get<ToolVm>($"api/tools/{id}"); return t == null ? RedirectToAction("Tools") : View(t); }
  [HttpPost] public async Task<IActionResult> Delete(int id) {
    var (ok, msg) = await api.Do(HttpMethod.Delete, $"api/tools/{id}"); TempData[ok ? "Ok" : "Error"] = ok ? "The tool was deleted." : msg; return RedirectToAction("Tools");
  }
  public async Task<IActionResult> Loans(bool? overdue) {
    ViewBag.Overdue = overdue == true;
    var loans = await api.Get<List<LoanVm>>("api/loans" + (overdue == true ? "?overdue=true" : "")); if (loans == null) ViewBag.Error = "We could not load the loans. Please try again.";
    return View(loans ?? new());
  }
}
