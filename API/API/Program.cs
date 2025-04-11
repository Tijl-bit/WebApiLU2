using Microsoft.EntityFrameworkCore;
using API.Models;
using API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using API;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.BearerToken;

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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories with the connection string
builder.Services.AddSingleton<IEnvironment2DRepository>(new Environment2DRepository(connectionString));
builder.Services.AddSingleton<IObject2DRepository>(new Object2DRepository(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Ensure Microsoft.EntityFrameworkCore.SqlServer package is installed

var app = builder.Build();

var sqlConnectionString = builder.Configuration.GetValue<string>("DefaultConnection");
var sqlConnectionStringFound = !string.IsNullOrWhiteSpace(sqlConnectionString);

app.UseAuthorization();
    

app.MapGroup("/accounts").MapIdentityApi<IdentityUser>();


app.MapGet("/", () => $"The API is up. Connection string found: {(sqlConnectionStringFound ? "true" : "false")}");

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();