using Microsoft.AspNetCore.Mvc; using ToolShare.Web.Filters; using ToolShare.Web.Models; using ToolShare.Web.Services;
namespace ToolShare.Web.Controllers;
[RequireRole]
public class LoansController(ApiClient api) : Controller {
  public async Task<IActionResult> Mine() {
    var loans = await api.Get<List<LoanVm>>("api/loans/mine");
    if (loans == null) ViewBag.Error = "We could not load your loans. Please try again in a moment.";
    return View(loans ?? new());
  }
  [HttpPost] public async Task<IActionResult> Return(int id, string name) {
    var (ok, msg) = await api.Do(HttpMethod.Post, $"api/loans/{id}/return");
    TempData[ok ? "Ok" : "Error"] = ok ? $"Thank you. {name} is back in the library." : msg; return RedirectToAction("Mine");
  }
}
