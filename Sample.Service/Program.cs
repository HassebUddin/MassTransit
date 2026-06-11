namespace Sample.Service
{
    // using Sample.Components.CourierActivities;
    // using Sample.Components.StateMachines;
    // using Sample.Components.StateMachines.OrderStateMachineActivities;
    using MassTransit;
    // using MassTransit.Courier.Contracts;
    // using MassTransit.MongoDbIntegration.MessageData;
    using MassTransit.RabbitMqTransport;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    // using Sample.Components.BatchConsumers;
    using Sample.Components.Consumers;
    using Sample.Components.StateMachines;
    using Sample.Components.StateMachines.StateMachineDefinitions;
    using Sample.Components.StateMachines.States;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    // using Warehouse.Contracts;

    class Program
    {
        static DependencyTrackingTelemetryModule _module;
        static TelemetryClient _telemetryClient;

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new HostBuilder()
                .UseEnvironment(isService ? Environments.Production : Environments.Development)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    _module = new DependencyTrackingTelemetryModule();
                    _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

                    TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                    configuration.InstrumentationKey = "6b4c6c82-3250-4170-97d3-245ee1449278";
                    configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                    _telemetryClient = new TelemetryClient(configuration);

                    _module.Initialize(configuration);

                    // services.AddScoped<AcceptOrderActivity>();
                    // services.AddScoped<RoutingSlipBatchEventConsumer>();

                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

                    var transport = hostContext.Configuration["MassTransit:Transport"] ?? "RabbitMQ";

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        // cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                        var mongoConnection = hostContext.Configuration["MongoDB:Connection"] ?? "mongodb://127.0.0.1:27017";
                        var mongoDatabase = hostContext.Configuration["MongoDB:DatabaseName"] ?? "orders";

                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = mongoConnection;
                                r.DatabaseName = mongoDatabase;
                            });

                        if (string.Equals(transport, "InMemory", StringComparison.OrdinalIgnoreCase))
                        {
                            cfg.UsingInMemory((context, configurator) =>
                            {
                                configurator.ConfigureEndpoints(context);
                            });
                        }
                        else
                        {
                            cfg.UsingRabbitMq(ConfigureBus);
                        }

                        // cfg.AddRequestClient<AllocateInventory>();
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                });

            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();

            _telemetryClient?.Flush();
            _module?.Dispose();

            Log.CloseAndFlush();
        }

        static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            var configuration = context.GetRequiredService<IConfiguration>();
            var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

            configurator.Host(rabbitHost, "/", h =>
            {
                h.Username(rabbitUser);
                h.Password(rabbitPass);
            });

            // configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));
            // configurator.UseMessageScheduler(new Uri("queue:quartz"));

            // configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
            // {
            //     e.PrefetchCount = 20;
            //
            //     e.Batch<RoutingSlipCompleted>(b =>
            //     {
            //         b.MessageLimit = 10;
            //         b.TimeLimit = TimeSpan.FromSeconds(5);
            //
            //         b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
            //     });
            // });

            configurator.ConfigureEndpoints(context);
        }
    }
}
