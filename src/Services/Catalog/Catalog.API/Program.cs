using Catalog.API.Data;
using Catalog.API.Repositories;
using Common.Logging;
using HealthChecks.UI.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICatalogContext, CatalogContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services
    .AddHealthChecks()
    .AddMongoDb(builder.Configuration["DatabaseSettings:ConnectionString"], "Catalog MongoDb Health", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

builder
    .Host
    .UseSerilog(Serilogger.Configure);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/hc", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
