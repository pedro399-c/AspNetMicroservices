using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Common.Logging;
using Discount.Grpc.Protos;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>
    (o => o.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]));

builder.Services.AddScoped<DiscountGrpcService>();

builder.Services
    .AddHealthChecks()
    .AddRedis(builder.Configuration["CacheSettings:ConnectionString"], "Redis Health", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded)
    .AddRabbitMQ(builder.Configuration["EventBusSettings:HostAddress"], null, "RabbitMQ Health", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, configurator) =>
    {
        configurator.Host(builder.Configuration["EventBusSettings:HostAddress"]);
    });
});

// builder.Services.AddMassTransitHostedService();

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
