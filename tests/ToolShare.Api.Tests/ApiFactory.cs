using Microsoft.AspNetCore.Hosting; using Microsoft.AspNetCore.Mvc.Testing; using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore; using Microsoft.Extensions.DependencyInjection;
using ToolShare.Api.Data;
/// <summary>Starts the API with an empty in-memory database. Create one per test so the first registered user (the Admin) is predictable.</summary>
public class ApiFactory : WebApplicationFactory<Program> {
  private readonly string _db = Guid.NewGuid().ToString();
  protected override void ConfigureWebHost(IWebHostBuilder b) {
    b.UseEnvironment("Development");
    b.ConfigureTestServices(s => {
      var old = s.Where(d => d.ServiceType == typeof(DbContextOptions<ToolShareContext>) ||
        (d.ServiceType.IsGenericType && d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration") && d.ServiceType.GenericTypeArguments[0] == typeof(ToolShareContext))).ToList();
      foreach (var d in old) s.Remove(d);
      s.AddDbContext<ToolShareContext>(o => o.UseInMemoryDatabase(_db));
    });
  }
}
