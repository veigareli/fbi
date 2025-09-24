using Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

// Default route for any other actions
app.MapControllerRoute(
    name: "default",
    pattern: "{action}",
    defaults: new { controller = "Home" });

// Map root path to Authentication Login
app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Authentication", action = "Login" });

// Map authentication routes
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Authentication", action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "register",
    defaults: new { controller = "Authentication", action = "Register" });

// Map home path (authenticated)
app.MapControllerRoute(
    name: "home",
    pattern: "home",
    defaults: new { controller = "Home", action = "Home" });

// Map myteam path (authenticated)
app.MapControllerRoute(
    name: "myteam",
    pattern: "myteam/{id?}",
    defaults: new { controller = "Home", action = "MyTeam" });

// Map rules path
app.MapControllerRoute(
    name: "rules",
    pattern: "rules",
    defaults: new { controller = "Home", action = "Rules" });

// Map API controllers
app.MapControllers();

app.Run();
