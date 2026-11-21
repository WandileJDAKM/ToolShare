using System.Net.Http.Json;
namespace ToolShare.Web.Services;
public class ApiClient {
  private readonly HttpClient _http; private readonly IHttpContextAccessor _ctx;
  public ApiClient(HttpClient http, IHttpContextAccessor ctx) { _http = http; _ctx = ctx; }
  void Attach(HttpRequestMessage r) { var c = _ctx.HttpContext!.Session.GetString("ApiCookie"); if (!string.IsNullOrEmpty(c)) r.Headers.Add("Cookie", c); }
  public async Task<HttpResponseMessage> Send(HttpMethod m, string url, object? body = null) {
    var r = new HttpRequestMessage(m, url); Attach(r);
    if (body != null) r.Content = JsonContent.Create(body);
    return await _http.SendAsync(r);
  }
  public async Task<T?> Get<T>(string url) {
    try { var res = await Send(HttpMethod.Get, url); return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<T>() : default; }
    catch { return default; }
  }
  // ok=true: msg is the response body. ok=false: msg is a friendly error.
  public async Task<(bool ok, string msg)> Do(HttpMethod m, string url, object? body = null) {
    try {
      var r = await Send(m, url, body); var t = await r.Content.ReadAsStringAsync();
      if (r.IsSuccessStatusCode) return (true, t);
      return (false, (int)r.StatusCode switch {
        401 => "Please log in again.", 403 => "You don't have permission to do that.", 404 => "That item no longer exists.",
        _ => t.Length is > 0 and < 200 ? t.Trim('"') : "Something went wrong. Please try again." });
    } catch { return (false, "The service is unavailable. Please try again shortly."); }
  }
  public async Task<bool> Upload(string url, IFormFile f) {
    try {
      var r = new HttpRequestMessage(HttpMethod.Post, url); Attach(r);
      var sc = new StreamContent(f.OpenReadStream()); sc.Headers.ContentType = new(f.ContentType);
      r.Content = new MultipartFormDataContent { { sc, "file", f.FileName } };
      return (await _http.SendAsync(r)).IsSuccessStatusCode;
    } catch { return false; }
  }
  public async Task<(bool ok, string? role, int userId)> Login(string email, string password) {
    try {
      var res = await Send(HttpMethod.Post, "api/auth/login", new { email, password });
      if (!res.IsSuccessStatusCode) return (false, null, 0);
      if (res.Headers.TryGetValues("Set-Cookie", out var v)) _ctx.HttpContext!.Session.SetString("ApiCookie", v.First().Split(';')[0]);
      var j = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
      _ctx.HttpContext!.Session.SetString("Name", j.GetProperty("name").GetString() ?? "");
      return (true, j.GetProperty("role").GetString(), j.GetProperty("id").GetInt32());
    } catch { return (false, null, 0); }
  }
}
