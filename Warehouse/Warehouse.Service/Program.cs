namespace Warehouse.Service
{
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Warehouse.Components.Consumers;
    using Warehouse.Components.StateMachines;
    using Warehouse.Components.StateMachines.StateMachineDefinitions;
    using Warehouse.Components.StateMachines.States;

    class Program
    {
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
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();

                        var mongoConnection = hostContext.Configuration["MongoDB:Connection"] ?? "mongodb://127.0.0.1:27017";
                        var mongoDatabase = hostContext.Configuration["MongoDB:DatabaseName"] ?? "warehouse";

                        cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocateStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = mongoConnection;
                                r.DatabaseName = mongoDatabase;
                            });

                        var schedulerEndpoint = new Uri("queue:scheduler");
                        cfg.AddMessageScheduler(schedulerEndpoint);

                        cfg.UsingRabbitMq((context, configurator) =>
                        {
                            ConfigureBus(context, configurator);
                        });
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

            configurator.UseMessageScheduler(new Uri("queue:scheduler"));

            configurator.ConfigureEndpoints(context);
        }
    }
}
