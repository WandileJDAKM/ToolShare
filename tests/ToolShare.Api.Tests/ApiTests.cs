using System.Net; using System.Net.Http.Json; using System.Text.Json; using Xunit;
public class ApiTests {
  static object Reg(string email) => new { name = "Test", email, password = "Password123!" };
  static async Task<HttpClient> Join(ApiFactory f, string email) {
    var c = f.CreateClient();
    (await c.PostAsJsonAsync("/api/auth/register", Reg(email))).EnsureSuccessStatusCode();
    (await c.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" })).EnsureSuccessStatusCode();
    return c;
  }
  static object NewTool(string name = "Drill") => new { name, description = "18V cordless", category = "Power tools" };
  static async Task<JsonElement> Json(HttpResponseMessage r) => await r.Content.ReadFromJsonAsync<JsonElement>();
  static async Task<int> Id(HttpResponseMessage r) => (await Json(r)).GetProperty("id").GetInt32();

  [Fact] public async Task FirstUserIsAdmin_ThenMembers() {
    using var f = new ApiFactory(); var c = f.CreateClient();
    var a = await Json(await c.PostAsJsonAsync("/api/auth/register", Reg("a@a.com")));
    var b = await Json(await c.PostAsJsonAsync("/api/auth/register", Reg("b@b.com")));
    Assert.Equal("Admin", a.GetProperty("role").GetString()); Assert.Equal("Member", b.GetProperty("role").GetString());
  }
  [Fact] public async Task Register_DuplicateEmail_Conflicts_ShortPassword_BadRequest() {
    using var f = new ApiFactory(); var c = f.CreateClient();
    Assert.Equal(HttpStatusCode.Created, (await c.PostAsJsonAsync("/api/auth/register", Reg("a@a.com"))).StatusCode);
    Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsJsonAsync("/api/auth/register", Reg("a@a.com"))).StatusCode);
    Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/register", new { name = "T", email = "x@x.com", password = "abc" })).StatusCode);
  }
  [Fact] public async Task Login_WrongPassword_Returns401() {
    using var f = new ApiFactory(); var c = f.CreateClient(); await c.PostAsJsonAsync("/api/auth/register", Reg("a@a.com"));
    Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/api/auth/login", new { email = "a@a.com", password = "wrong-password" })).StatusCode);
  }
  [Fact] public async Task Unauthenticated_Returns401() {
    using var f = new ApiFactory();
    Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().GetAsync("/api/tools")).StatusCode);
  }
  [Fact] public async Task Logout_ClearsSession() {
    using var f = new ApiFactory(); var c = await Join(f, "a@a.com");
    Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/api/tools")).StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, (await c.PostAsync("/api/auth/logout", null)).StatusCode);
    Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/tools")).StatusCode);
  }
  [Fact] public async Task Member_CannotCreateTool_Admin_Can() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com"); var member = await Join(f, "m@m.com");
    Assert.Equal(HttpStatusCode.Forbidden, (await member.PostAsJsonAsync("/api/tools", NewTool())).StatusCode);
    Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/tools", NewTool())).StatusCode);
  }
  [Fact] public async Task Search_FiltersTools() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com");
    await admin.PostAsJsonAsync("/api/tools", NewTool("Drill")); await admin.PostAsJsonAsync("/api/tools", NewTool("Saw"));
    Assert.Equal(1, (await admin.GetFromJsonAsync<JsonElement>("/api/tools?search=dri")).GetArrayLength());
  }
  [Fact] public async Task Borrow_MakesToolUnavailable_SecondBorrowConflicts() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com"); var m = await Join(f, "m@m.com");
    var tid = await Id(await admin.PostAsJsonAsync("/api/tools", NewTool()));
    Assert.Equal(HttpStatusCode.Created, (await m.PostAsJsonAsync($"/api/tools/{tid}/borrow", new { days = 7 })).StatusCode);
    Assert.False((await m.GetFromJsonAsync<JsonElement>($"/api/tools/{tid}")).GetProperty("isAvailable").GetBoolean());
    Assert.Equal(HttpStatusCode.Conflict, (await m.PostAsJsonAsync($"/api/tools/{tid}/borrow", new { days = 7 })).StatusCode);
  }
  [Fact] public async Task Borrow_InvalidDays_Returns400() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com");
    var tid = await Id(await admin.PostAsJsonAsync("/api/tools", NewTool()));
    Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync($"/api/tools/{tid}/borrow", new { days = 99 })).StatusCode);
  }
  [Fact] public async Task Return_ByBorrower_FreesTool_ByOtherMember_Forbidden() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com"); var m1 = await Join(f, "m1@m.com"); var m2 = await Join(f, "m2@m.com");
    var tid = await Id(await admin.PostAsJsonAsync("/api/tools", NewTool()));
    var lid = await Id(await m1.PostAsJsonAsync($"/api/tools/{tid}/borrow", new { days = 3 }));
    Assert.Equal(HttpStatusCode.Forbidden, (await m2.PostAsync($"/api/loans/{lid}/return", null)).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await m1.PostAsync($"/api/loans/{lid}/return", null)).StatusCode);
    Assert.True((await m1.GetFromJsonAsync<JsonElement>($"/api/tools/{tid}")).GetProperty("isAvailable").GetBoolean());
  }
  [Fact] public async Task AllLoans_AdminOnly() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com"); var m = await Join(f, "m@m.com");
    Assert.Equal(HttpStatusCode.Forbidden, (await m.GetAsync("/api/loans")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/loans")).StatusCode);
  }
  [Fact] public async Task Delete_ToolWithLoanHistory_Conflicts() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com");
    var tid = await Id(await admin.PostAsJsonAsync("/api/tools", NewTool()));
    var lid = await Id(await admin.PostAsJsonAsync($"/api/tools/{tid}/borrow", new { days = 2 }));
    await admin.PostAsync($"/api/loans/{lid}/return", null);
    Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/api/tools/{tid}")).StatusCode);
  }
  [Fact] public async Task Delete_UnusedTool_Succeeds() {
    using var f = new ApiFactory(); var admin = await Join(f, "admin@a.com");
    var tid = await Id(await admin.PostAsJsonAsync("/api/tools", NewTool()));
    Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/tools/{tid}")).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/tools/{tid}")).StatusCode);
  }
}
