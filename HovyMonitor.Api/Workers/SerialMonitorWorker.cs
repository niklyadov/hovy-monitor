using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using HovyMonitor.Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HovyMonitor.Api.Workers
{
    public class SerialMonitorWorker : BackgroundService
    {
        private readonly ILogger<SerialMonitorWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SerialMonitor _serialMonitor;

        public SerialMonitorWorker(ILogger<SerialMonitorWorker> logger,
            IServiceScopeFactory serviceScopeFactory, SerialMonitor serialMonitor)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _serialMonitor = serialMonitor;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serialMonitor.StartAsync(stoppingToken);
            _logger.LogInformation("Init success!");
           
            using var scope = _serviceScopeFactory.CreateScope();
            var sensorDetections = scope.ServiceProvider.GetRequiredService<SensorDetectionsService>();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var commandDht11 = new CommandAwaiter("dht11_dt");
                commandDht11.CommandResponseReceived += async response =>
                    await sensorDetections.WriteDetectionsAsync(response);
                
                _serialMonitor.SendCommand(commandDht11);
                
                var commandMhz19 = new CommandAwaiter("mhz19_dt");
                commandMhz19.CommandResponseReceived += async response =>
                    await sensorDetections.WriteDetectionsAsync(response);
                
                _serialMonitor.SendCommand(commandMhz19);

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}