using Hammer.Support.Infrastructure;
using Hammer.Support.Infrastructure.Configuration;
using Scalar.AspNetCore;
using Serilog;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var envFile = environment switch
{
    "Development" => ".env.local",
    "Staging" => ".env.dev",
    _ => ".env.live",
};
EnvFileLoader.Load(envFile);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapControllers();
}

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");

await app.RunAsync();
