using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HovyMonitor.Api.Entity;
using HovyMonitor.Api.Workers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HovyMonitor.Api.Services;

public class SerialMonitor
{
    private readonly Queue<CommandAwaiter> _commands = new();
    private readonly SerialPortConfiguration _configuration;
    private readonly ILogger<SerialMonitor> _logger;
    private DateTime? _lastCommandExecutionTime = null;
    private Task _commandHandleTask = null;

    private SerialPort _port;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _commandHandleTaskCts;
    
    public SerialMonitor(IOptions<Configuration> configuration, ILogger<SerialMonitor> logger)
    {
        _configuration = configuration.Value.SerialPort;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        
        await Task.Run(HealthCheck, _cancellationToken);
    }
    
    public void Send(CommandAwaiter command)
    {
        _commands.Enqueue(command);
        _logger.LogDebug("Command {CommandName} enqueued to queue", command.CommandName);
    }

    #region Private

    private async Task HealthCheck()
    {
        while(!_cancellationToken.IsCancellationRequested)
        {
            if (!_lastCommandExecutionTime.HasValue)
            {
                _logger.LogDebug("Commands listener is not started");
                await StartHandleCommands();
            }
            else
            {
                var lastCmdDiffTime = DateTime.UtcNow - _lastCommandExecutionTime.Value;

                _logger.LogDebug("Health {HealthType} {SecondsSinceCommandExecuted}", lastCmdDiffTime.TotalSeconds < 5 ? "good": "bad", lastCmdDiffTime.TotalSeconds);

                if (lastCmdDiffTime.TotalSeconds > 10)
                {
                    _logger.LogWarning("Commands not executed for many time ({TimeDifference})", lastCmdDiffTime);
                    await RestartHandleCommands();
                }
            }

            await Task.Delay(3000, _cancellationToken);
        }
    }

    private async Task StartHandleCommands()
    {
        if(_port == null || !_port.IsOpen)
        {
            _port = await GetArduinoPort();
        }

        _logger.LogDebug("Starting a Commands listener");
        _commandHandleTaskCts = new CancellationTokenSource();
        _commandHandleTask = Task.Run(HandleCommands, _commandHandleTaskCts.Token);
    }
    
    private async Task RestartHandleCommands()
    {
        _port.Close();
        _port.Dispose();

        _commandHandleTaskCts.Cancel();
        _commandHandleTask.Dispose();
        _lastCommandExecutionTime = null;

        await StartHandleCommands();
    }

    private async Task HandleCommands()
    {
        while(!_cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_configuration.SendInterval, _cancellationToken);

            if (!_commands.Any()) continue;
            
            var command = _commands.Dequeue();
            _logger.LogDebug("Working with a command {CommandName}", command.CommandName);
                    
            SendCommand(_port, command.CommandName);
            _logger.LogDebug("Success send command {CommandName} to board", command.CommandName);
                    
            command.CommandResponse = await WaitCommandResponse(_cancellationToken);
                    
            _logger.LogDebug("Success receive a data for command {CommandName}", command.CommandName);
                    
            _lastCommandExecutionTime = DateTime.UtcNow;
        }
    }

    private async Task<string> WaitCommandResponse(CancellationToken cancellationToken, int countRetries = 2, int currentRetry = 1)
    {
        _logger.LogDebug("Wait command response, retry: ({Retry}/{TotalRetries})",currentRetry, countRetries);
        
        if (currentRetry >= countRetries)
        {
            return string.Empty;
        }

        await Task.Delay(_configuration.ReadTimeout / countRetries, cancellationToken);

        if (_port == null)
        {
            throw new Exception("Port is null");
        }
        
        if (!_port.IsOpen)
        {
            throw new Exception("Serial port is closed");
        } 

        var responseString = _port.ReadExisting();

        if(!string.IsNullOrEmpty(responseString))
        {
            return responseString;
        }

        return await WaitCommandResponse(cancellationToken, countRetries, currentRetry + 1);
    }

    private async Task<SerialPort> GetArduinoPort()
    {
        var serialPortNames = SerialPort.GetPortNames();
        _logger.LogDebug("Search an arduino port. Total ports count {PortsCount}", serialPortNames.Length);
        foreach (var portName in serialPortNames)
        {
            var serialPort = new SerialPort(portName, _configuration.BaundRate);
            serialPort.DataBits = _configuration.DataBits;

            serialPort.PinChanged += (sender, args) =>
            {
                _logger.LogDebug("Wow, pin changed");
            };

            if(await CheckArduinoPort(serialPort))
            {
                _logger.LogDebug("Port with name {PortName} is a arduino port", portName);
                return serialPort;
            }
            
            _logger.LogDebug("Port with name {PortName} is a not arduino port", portName);
        }

        await Task.Delay(500);
        return await GetArduinoPort();
    }

    private async Task<bool> CheckArduinoPort(SerialPort serialPort, int count = 5)
    {
        try
        {
            serialPort.ReadTimeout = 1000;
            serialPort.ErrorReceived += (_, _) 
                => throw new Exception("Some error with serial");
           
            serialPort.Open();

            if (count <= 0)
            {
                throw new Exception("Time out");
            }

            if (await PingArduinoBoard(serialPort))
            {
                return true;
            }

            if (await PingArduinoBoard(serialPort))
            {
                return true;
            }

            return await CheckArduinoPort(serialPort, count - 1);
        }
        catch
        {
            serialPort.Close();
            serialPort.Dispose();
            return false;
        }
    }

    private async Task<bool> PingArduinoBoard(SerialPort serialPort)
    {
        const string ping = "ping";
        const string pong = "pong";

        SendCommand(serialPort, ping);

        await Task.Delay(_configuration.SendInterval, _cancellationToken);

        return serialPort.ReadExisting().Equals(pong, StringComparison.OrdinalIgnoreCase);
    }

    private void SendCommand(SerialPort port, string commandName)
    {
        var maxCommandLength = 10;

        // start chars for UBC
        var chars = new List<char>()
        {
            (char)0x0a, (char)0x00
        };

        chars.AddRange(commandName.ToCharArray());

        if (chars.Count > maxCommandLength)
        {
            throw new InvalidOperationException("Command must be less or equal 10 chars");
        }

        for (int i = 0; i < maxCommandLength - commandName.Length; i++)
        {
            chars.Add('\0');
        }

        if (!port.IsOpen)
        {
            throw new Exception("Serial port is closed");
        } 
        port.Write(chars.ToArray(), 0, chars.Count);
    }
    #endregion
}
