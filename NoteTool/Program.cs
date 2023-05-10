using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NoteTool.Infrastructure;
using NoteTool.Stub;
using Spectre.Console;
using Spectre.Console.Cli;

AnsiConsole.MarkupLine("[blue]Note Tool[/]");
AnsiConsole.WriteLine();

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddTransient<IConfiguration>(_ => configuration);
services.Configure<WorkLogOptions>(configuration.GetSection(nameof(WorkLogOptions)));

services.AddTransient<IClock>(_ => SystemClock.Instance);

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<StubWeeklyFolderCommand>("stub")
        .WithExample(new[] { "--week-starting \"2023-05-01\"" });
});
return await app.RunAsync(args);
