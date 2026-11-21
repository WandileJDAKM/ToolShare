using Microsoft.AspNetCore.Mvc; using ToolShare.Web.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!))
  .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
var app = builder.Build();
app.UseStaticFiles(); app.UseRouting(); app.UseSession();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
