using CommandLine;
using FileSynchronizer.Abstracts;
using FileSynchronizer.Configuration;
using FileSynchronizer.Extensions;
using FileSynchronizer.Extensions.DependencyInjection;
using Hangfire;
using System.ComponentModel.DataAnnotations;


var parser = new Parser(with => with.IgnoreUnknownArguments = true);
var parserResultOptions = parser.ParseArguments<ApplicationOptions>(args);

if (parserResultOptions is not Parsed<ApplicationOptions> parsedOptions)
{
    throw new ArgumentException($"Invalid command-line arguments provided. {string.Join(" ", args)}");
}

var validationResults = new List<ValidationResult>();
if (!Validator.TryValidateObject(parsedOptions.Value, new ValidationContext(parsedOptions.Value), validationResults, validateAllProperties: true))
{
    Console.WriteLine($"Chyba konfigurace: {validationResults[0].ErrorMessage}");
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureHangfire();
builder.Host
    .ConfigureSerilog(parsedOptions.Value)
    .ConfigureServices(parsedOptions.Value);

var app = builder.Build();

app.UseHangfireDashboard();

var appManager = app.Services.GetRequiredService<IApplicationOrchestrator>();
appManager.Start();

await app.RunAsync();
