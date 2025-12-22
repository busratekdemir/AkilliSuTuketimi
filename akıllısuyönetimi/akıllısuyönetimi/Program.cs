// Program.cs
using akıllısuyönetimi.Data;
using akıllısuyönetimi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// LOG AYARLARI (ÇOK ÖNEMLİ)
// ====================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ====================================================================
// BAĞIMLILIK ENJEKSİYONU (DI)
// ====================================================================
builder.WebHost.UseWebRoot("wwwroot");
builder.Services.AddControllersWithViews();

// Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
     ?? throw new InvalidOperationException(
         "Connection string 'DefaultConnection' not found in appsettings.json.");

Console.WriteLine("KULLANILAN CONNECTION STRING:");
Console.WriteLine(connectionString);

// DbContext + EF CORE SQL LOG
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // SQL parametrelerini de gösterir
           .LogTo(Console.WriteLine, LogLevel.Information);
});

// ====================================================================
// AUTHENTICATION
// ====================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// ====================================================================
// AUTHORIZATION
// ====================================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});

// ====================================================================
// PIPELINE
// ====================================================================
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
