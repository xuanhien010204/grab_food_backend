using FoodOrderingCore.Context;
using FoodOrderingPRM392.Extension;
using FoodOrderingPRM392.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration Configuration = builder.Configuration;

builder.Services.AddDbContext<FoodOrderingContext>(o => o.UseSqlServer(Configuration.GetConnectionString("FOOD"), b => b.MigrationsAssembly("FoodOrderingPRM392")));

// Configure application options (ConnectionStrings, MoMo, etc.)
builder.Services.ConfigureApplicationOptions(Configuration);

// Configure repositories and services
builder.Services.ConfigureRepositories();

// Configure HttpClients for external APIs (MoMo, etc.)
builder.Services.ConfigureHttpClients(Configuration);

builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "auth_cookie";
    options.SlidingExpiration = false;
    options.ExpireTimeSpan = new TimeSpan(168, 0, 0);
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events.OnRedirectToAccessDenied =
    options.Events.OnRedirectToLogin = async c =>
    {
        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await Task.CompletedTask;
    };

    options.Events.OnRedirectToLogout = async action =>
    {
        action.Response.StatusCode = StatusCodes.Status200OK;
        await Task.CompletedTask;
    };
});

// Controllers with optional ExceptionFilter (middleware will handle most exceptions)
builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global Exception Handler (catches all exceptions)
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS Redirection (Optional)
//app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
