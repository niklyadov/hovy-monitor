using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.IO.Ports;
using System.Threading.Tasks;
using HovyMonitor.Api.Services;
using Microsoft.Extensions.Options;
using HovyMonitor.Api.Entity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HovyMonitor.Api.Workers
{
    public class SerialMonitorWorker : BackgroundService
    {
        private readonly ILogger<SerialMonitorWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Configuration _configuration;
        private SerialPort _port;
        private DateTime _lifeDateTime;

        public SerialMonitorWorker(ILogger<SerialMonitorWorker> logger,
            IServiceScopeFactory serviceScopeFactory, IOptions<Configuration> configuration)
        {
            _logger = logger;

            _serviceScopeFactory = serviceScopeFactory;

            _configuration = configuration.Value;

            _logger.LogInformation("Init success!");

        }

        private SerialPort RegisterPort()
        {
            var port = new SerialPort(_configuration.SerialPort.Name,
                _configuration.SerialPort.BaundRate, Parity.None,
                _configuration.SerialPort.DataBits, StopBits.One);
            port.ReadTimeout = _configuration.SerialPort.ReadTimeout;
            port.DtrEnable = true;
            port.RtsEnable = true;
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceivedAsync);
            _lifeDateTime = DateTime.Now;

            return port;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var totalSecondsNoResponse = (DateTime.Now - _lifeDateTime).TotalSeconds;
                if (_port != null && totalSecondsNoResponse > 10)
                {
                    _logger.LogError($"No messages received {totalSecondsNoResponse} seconds");
                }

                if (_port != null && totalSecondsNoResponse > 20)
                {
                    _logger.LogError($"Reconnecting, because no messages at more than 20 seconds");

                    ReconnectPort();
                }

                if (_port == null || (_port != null && !_port.IsOpen))
                {
                    ReconnectPort();
                }

                await Task.Delay(3000, stoppingToken);
            }
        }
        private async void port_DataReceivedAsync(object sender, SerialDataReceivedEventArgs e)
        {
            string readedStr;

            try
            {
                readedStr = _port.ReadLine();
            } catch(Exception ex)
            {
                _logger.LogError($"Read error {ex.Message}");
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var sensorDetections = scope.ServiceProvider.GetRequiredService<SensorDetectionsService>();
            await sensorDetections.WriteDetectionsAsync(readedStr);

            _logger.LogDebug($"Data received: {readedStr}");

            _lifeDateTime = DateTime.Now;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_port != null && _port.IsOpen)
            {
                _port.Close();
            }
        }

        private bool ConnectToPort()
        {
            try
            {
                _port.Open();
                _logger.LogInformation("Listening started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connect error {ex.Message}");
                return false;
            }
            
            _logger.LogDebug($"Opened Port Info:: IsOpen: {_port.IsOpen}, Handshake: {_port.Handshake}");

            return true;
        }

        private bool ReconnectPort()
        {
            if (_port != null && _port.IsOpen) _port.Close();
            _port = RegisterPort();
            return ConnectToPort();
        }
    }
}