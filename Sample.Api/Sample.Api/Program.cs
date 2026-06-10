using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Components.Consumers;
using Sample.Components.StateMachines.States;
using Sample.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddMassTransit(mt =>
{
    // API sends requests; Sample.Service consumes on submit-order queue
    mt.AddRequestClient<SubmitOrder>(
        new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}"));
    mt.AddRequestClient<CheckOrder>(
      new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Saga<OrderState>()}"));

    mt.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
