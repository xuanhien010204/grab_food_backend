using FoodOrderingCore.Context;
using FoodOrderingPRM392.Extension;
using FoodOrderingPRM392.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration Configuration = builder.Configuration;

builder.Services.AddDbContext<FoodOrderingContext>(o => o.UseSqlServer(Configuration.GetConnectionString("FOOD"), b => b.MigrationsAssembly("FoodOrderingPRM392")));

builder.Services.ConfigureApplicationOptions(Configuration);
builder.Services.ConfigureRepositories();

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
    options.ExpireTimeSpan = new TimeSpan(24, 0, 0);

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


builder.Services.AddControllers(option => option.Filters.Add<ExceptionFilter>()).AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
