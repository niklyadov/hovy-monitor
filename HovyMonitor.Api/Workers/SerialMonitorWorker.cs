using HovyMonitor.Api.Entity;
using HovyMonitor.Api.Services;
using Microsoft.Extensions.Options;

namespace HovyMonitor.Api.Workers
{
    public class SerialMonitorWorker : BackgroundService
    {
        private readonly ILogger<SerialMonitorWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SerialMonitor _serialMonitor;
        private readonly Configuration _configuration;

        public SerialMonitorWorker(ILogger<SerialMonitorWorker> logger,
            IServiceScopeFactory serviceScopeFactory, SerialMonitor serialMonitor, IOptions<Configuration> cfgOptions)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _serialMonitor = serialMonitor;
            _configuration = cfgOptions.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serialMonitor.StartAsync(stoppingToken);
            _logger.LogInformation("Init success!");
           
            using var scope = _serviceScopeFactory.CreateScope();
            var sensorDetections = scope.ServiceProvider.GetRequiredService<SensorDetectionsService>();

            var commands = _configuration.CommandsForSurveysOfSensors;
            
            if(!commands.Any()) 
                _logger.LogWarning("No commands specified for survey");
            else
            {
                _logger.LogInformation("{CommandsCount} commands specified, is a: {CommandNames}", commands.Count, String.Join(';', commands));
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (String commandName in commands)
                {
                    var commandTask = new CommandAwaiter(commandName);
                    commandTask.CommandResponseReceived += async response =>
                        await sensorDetections.WriteDetectionsAsync(response);
                    _serialMonitor.Send(commandTask);
                }
                
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}