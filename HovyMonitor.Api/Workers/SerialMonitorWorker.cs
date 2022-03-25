using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.IO.Ports;
using System.Threading.Tasks;
using System;
using HovyMonitor.Api.Services;
using Microsoft.Extensions.Options;
using HovyMonitor.Api.Entity;

namespace HovyMonitor.Api.Workers
{
    public class SerialMonitorWorker : BackgroundService
    {
        private readonly ILogger<SerialMonitorWorker> _logger;
        private readonly SensorDetectionsService _sensorDetections;
        private readonly Configuration _configuration;
        private readonly SerialPort _port;

        public SerialMonitorWorker(ILogger<SerialMonitorWorker> logger, 
            SensorDetectionsService sensorDetections, IOptions<Configuration> configuration)
        {
            _logger = logger;
            _sensorDetections = sensorDetections;
            _configuration = configuration.Value;
            _port = new SerialPort(_configuration.SerialPort.Name, 
                _configuration.SerialPort.BaundRate, Parity.None, 
                _configuration.SerialPort.DataBits, StopBits.One);
            _port.ReadTimeout = _configuration.SerialPort.ReadTimeout;
            _port.DtrEnable = true;
            _port.RtsEnable = true;
            _port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _port.Open();

                await Task.Delay(-1, stoppingToken);
            }
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var readedStr = _port.ReadLine();

            _sensorDetections.WriteDetections(readedStr);
            _logger.LogDebug($"Data received: {readedStr}");
        }
    }
}
