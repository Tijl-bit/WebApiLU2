using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Models;
using API.Repositories;
using API.Services;
using API;

var builder = WebApplication.CreateBuilder(args);

// ✅ Use the proper way to get the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"🔍 Checking SQL Connection: {(string.IsNullOrWhiteSpace(connectionString) ? "Not Found" : "Found")}");

// 🔐 Identity and Authentication
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.Tokens.AuthenticatorTokenProvider = null;

        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddDapperStores(options =>
    {
        options.ConnectionString = connectionString;
    });

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
    .Configure(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromMinutes(60);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthenticationService, AspNetIdentityAuthenticationService>();

// ✅ Register database and custom repositories
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<IEnvironment2DRepository>(new Environment2DRepository(connectionString));
builder.Services.AddSingleton<IObject2DRepository>(new Object2DRepository(connectionString));

// ✅ Add basic services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Logging connection string status
var sqlConnectionStringFound = !string.IsNullOrWhiteSpace(connectionString);

// 🔧 Error handling, Swagger, etc.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Correct middleware order
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("/accounts").MapIdentityApi<IdentityUser>();

app.MapPost("/logout", async (
    SignInManager<IdentityUser> signInManager,
    [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }

    return Results.Unauthorized();
});

// 🔍 Root health check
app.MapGet("/", () => $"The API is up. Connection string found: {(sqlConnectionStringFound ? "true" : "false")}");

app.Run();
