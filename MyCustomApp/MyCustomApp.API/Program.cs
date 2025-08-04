
using MyCustomApp.API;
using MyCustomApp.API.Extensions;
using MyCustomApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.ConfigurationKeys;
using Shared.Web.Middlewares;
WebApplicationBuilder builder = WebApplication.CreateBuilder();

//Default methods
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Configuration.AddJsonFile("appsettings.json");
//Extension Methods
builder.Configuration.AddKeyVault();
builder.Services.SetupSwagger(ConfigurationKeys.Assemblies.AppName);
builder.Services.SetupLogging(builder.Environment, builder.Configuration);
builder.Services.SetupCors(builder.Environment);
builder.Services.SetupInfrastructureDependencies(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddApplicationDependencies(builder.Configuration);
builder.Services.SetupApplicationDependencies(builder.Configuration);


var app = builder.Build();


//Endpoints Extension Methods
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(ConfigurationKeys.CrossOriginResourceSharing.PolicyName);
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();

// Enhanced health check endpoints with detailed response formatting
app.MapHealthChecks("/ping", new HealthCheckOptions
{
    Predicate = check => check.Name.Contains("Ping")
}.WithDetailedResponse());

app.MapHealthChecks("/warmup", new HealthCheckOptions
{
    Predicate = check => check.Name.Contains("Warmup")
}.WithDetailedResponse());

// Comprehensive health check endpoint (both ping + warmup)
app.MapHealthChecks("/health", new HealthCheckOptions().WithDetailedResponse());

// Simple ready/live endpoints for Kubernetes-style probes
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Name.Contains("Ping"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
}.WithSimpleResponse());

app.MapHealthChecks("/live", new HealthCheckOptions
{
    Predicate = _ => false // No checks, just returns if the app is running
}.WithSimpleResponse());

app.MapAllEndpoints();
app.Run();
