using Microsoft.EntityFrameworkCore; using ToolShare.Api.Data; using ToolShare.Api.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToolShareContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { var xml = Path.Combine(AppContext.BaseDirectory, "ToolShare.Api.xml"); if (File.Exists(xml)) c.IncludeXmlComments(xml); });
var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
  var db = scope.ServiceProvider.GetRequiredService<ToolShareContext>();
  db.Database.EnsureCreated();
  if (app.Configuration.GetValue<bool>("Seed:Demo") && !db.Tools.Any()) {
    db.Tools.AddRange(
      new Tool { Name = "Cordless drill", Description = "18V drill with two batteries", Category = "Power tools" },
      new Tool { Name = "Step ladder", Description = "2m aluminium ladder", Category = "Access" },
      new Tool { Name = "Pressure washer", Description = "1800W, includes hose", Category = "Cleaning" },
      new Tool { Name = "Tile cutter", Description = "600mm manual cutter", Category = "Hand tools" });
    db.SaveChanges();
  }
}

app.UseSwagger(); app.UseSwaggerUI();
app.UseSession();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();
app.Run();

public partial class Program { }
