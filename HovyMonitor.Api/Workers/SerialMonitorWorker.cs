using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.IO.Ports;
using System.Threading.Tasks;
using HovyMonitor.Api.Services;
using Microsoft.Extensions.Options;
using HovyMonitor.Api.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace HovyMonitor.Api.Workers
{
    public class SerialMonitorWorker : BackgroundService
    {
        private readonly ILogger<SerialMonitorWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Configuration _configuration;
        private readonly SerialPort _port;

        public SerialMonitorWorker(ILogger<SerialMonitorWorker> logger,
            IServiceScopeFactory serviceScopeFactory, IOptions<Configuration> configuration)
        {
            _logger = logger;

            _serviceScopeFactory = serviceScopeFactory;

            _configuration = configuration.Value;

            _port = new SerialPort(_configuration.SerialPort.Name, 
                _configuration.SerialPort.BaundRate, Parity.None, 
                _configuration.SerialPort.DataBits, StopBits.One);
            _port.ReadTimeout = _configuration.SerialPort.ReadTimeout;
            _port.DtrEnable = true;
            _port.RtsEnable = true;
            _port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceivedAsync);

            _logger.LogInformation("Starting Listening");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _port.Open();

                _logger.LogDebug($"Opened Port Info:: IsOpen: {_port.IsOpen}, Handshake: {_port.Handshake}");

                await Task.Delay(-1, stoppingToken);
            }
        }

        private async void port_DataReceivedAsync(object sender, SerialDataReceivedEventArgs e)
        {
            var readedStr = _port.ReadLine();

            using var scope = _serviceScopeFactory.CreateScope();
            var sensorDetections = scope.ServiceProvider.GetRequiredService<SensorDetectionsService>();
            await sensorDetections.WriteDetectionsAsync(readedStr);

            _logger.LogDebug($"Data received: {readedStr}");
        }
    }
}