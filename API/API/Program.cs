using Microsoft.EntityFrameworkCore;
using API.Models;
using API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Services;  // Correct namespace for your custom service

var builder = WebApplication.CreateBuilder(args);

// Log connection string status at startup
var connectionString = builder.Configuration["DefaultConnection"];
Console.WriteLine($"🔍 Checking SQL Connection: {(!string.IsNullOrWhiteSpace(connectionString) ? "Found" : "Not Found")}");

builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>(options => {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddDapperStores(options => { options.ConnectionString = connectionString; });

builder.Services
    .AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
    .Configure(options => {
        options.BearerTokenExpiration = TimeSpan.FromMinutes(60);
    });

builder.Services.AddHttpContextAccessor();

// Register your authentication service using full namespace:
builder.Services.AddScoped<API.Services.IAuthenticationService, API.Services.AspNetIdentityAuthenticationService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories with the connection string
builder.Services.AddSingleton<IEnvironment2DRepository>(new Environment2DRepository(connectionString));
builder.Services.AddSingleton<IObject2DRepository>(new Object2DRepository(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

var sqlConnectionString = builder.Configuration.GetValue<string>("DefaultConnection");
var sqlConnectionStringFound = !string.IsNullOrWhiteSpace(sqlConnectionString);

app.UseAuthorization();
app.MapGroup("/accounts").MapIdentityApi<IdentityUser>();

app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager,
    [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }
    return Results.Unauthorized();
});

app.MapGet("/", () => $"The API is up. Connection string found: {(sqlConnectionStringFound ? "true" : "false")}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
