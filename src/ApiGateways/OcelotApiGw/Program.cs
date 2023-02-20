using Common.Logging;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOcelot()
    .AddCacheManager(settings => settings.WithDictionaryHandle());

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile($"ocelot.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
});

/*
builder.Host.ConfigureLogging((hostingContext, loggingBuilder) =>
{
    loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});
*/

builder
    .Host
    .UseSerilog(Serilogger.Configure);

var app = builder.Build();

await app.UseOcelot();

app.MapGet("/", () => "Hello World!");

app.Run();
