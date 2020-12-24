using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleAppWithSerilog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args)
                    .RunConsoleAsync();Log.CloseAndFlush();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, logging) =>
                {
                    var configuration = hostContext.Configuration;

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

                    // Remove all default logging providers
                    logging.ClearProviders();
                    logging.AddSerilog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<App>();
                });
    }

    public class App : IHostedService
    {
        private readonly ILogger<App> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public App(ILogger<App> logger, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _appLifetime = applicationLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted, cancellationToken);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void OnStarted(object? obj)
        {
            var cancellationToken = (CancellationToken) (obj ?? default(CancellationToken));

            _logger.LogInformation("App started at: {time}", DateTimeOffset.Now);

            await Task.Delay(500, cancellationToken);

            _logger.LogTrace("Log Trace");
            _logger.LogDebug("Log Debug");
            _logger.LogInformation("Log Information");
            _logger.LogWarning("Log Warning");
            _logger.LogError("Log Error");
            _logger.LogCritical("Log Critical");

            _appLifetime.StopApplication();
        }

        private void OnStopping()
        {
            _logger.LogInformation("App stopping at: {time}", DateTimeOffset.Now);
        }

        private void OnStopped()
        {
            _logger.LogInformation("App stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
