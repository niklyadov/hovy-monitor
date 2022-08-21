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
    private readonly Stack<CommandAwaiter> _commands = new();
    private readonly SerialPortConfiguration _configuration;
    private readonly ILogger<SerialMonitor> _logger;
    
    private SerialPort _port;
    private CancellationToken _cancellationToken;
    
    public SerialMonitor(IOptions<Configuration> configuration, ILogger<SerialMonitor> logger)
    {
        _configuration = configuration.Value.SerialPort;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _port = await GetArduinoPort();

        if(_port == null)
        {
            throw new Exception("Can`t find arduino port");
        }

        await Task.Run(async () =>
        {
            while(!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_configuration.SendInterval, _cancellationToken);
                
                if(_commands.Any())
                {
                    var command = _commands.Pop();
                    _logger.LogDebug("Work with a command {CommandName}", command.CommandName);
                    
                    SendCommand(_port, command.CommandName);
                    command.CommandResponse = await WaitCommandResponse(_cancellationToken);
                }
            }
        }, _cancellationToken);
    }
    
    public void SendCommand(CommandAwaiter command)
    {
        _commands.Push(command);
        
        _logger.LogDebug("Command {CommandName} pushed to stack", command.CommandName);
    }

    #region Private
    private async Task<string> WaitCommandResponse(CancellationToken cancellationToken, int countRetries = 2, int currentRetry = 0)
    {
        if (currentRetry > countRetries)
        {
            _logger.LogDebug("Stop waiting response retry ({Retry}/{TotalRetries}",currentRetry, countRetries );
            return string.Empty;
        }

        await Task.Delay(_configuration.ReadTimeout / countRetries, cancellationToken);

        if (_port == null)
        {
            throw new Exception("Port is null");
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
        _logger.LogDebug("Search a arduino port. Total ports {Ports}", serialPortNames.Length);
        foreach (var portName in serialPortNames)
        {
            var serialPort = new SerialPort(portName, _configuration.BaundRate);
            serialPort.DataBits = _configuration.DataBits;

            if(await CheckArduinoPort(serialPort))
            {
                _logger.LogDebug("Port with name {PortName} is a arduino port",portName);
                return serialPort;
            }
            
            _logger.LogDebug("Port with name {PortName} is a not arduino port",portName);
        }

        return null;
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

    private void SendCommand(SerialPort _port, string commandName)
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
            throw new InvalidOperationException("Command must be more than 10 chars");
        }

        for (int i = 0; i < maxCommandLength - commandName.Length; i++)
        {
            chars.Add('\0');
        }


        _port.Write(chars.ToArray(), 0, chars.Count);
    }
    #endregion
}
