using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HovyMonitor.Api.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HovyMonitor.Api.Workers;

public class SerialMonitor
{
    private const int _sendMessagesIntervalMs = 1000;
    private const int _readMessagesTimeoutMs = 1000;
    private SerialPort? _port;
    private Stack<CommandAwaiter> _commands = new();
    private Task? _backgroundTask = null;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private readonly SerialPortConfiguration _configuration;
    private readonly ILogger<SerialMonitor> _logger;
    
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
                await Task.Delay(_sendMessagesIntervalMs, _cancellationToken);
                
                if(_commands.Any())
                {
                    var commandAwaiter = _commands.Pop();

                    SendCommand(_port, commandAwaiter.CommandName);

                    commandAwaiter.CommandResponse = await WaitCommandResponse(_cancellationToken);
                }
            }
        }, _cancellationToken);
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task<string> WaitCommandResponse(CancellationToken cancellationToken, int countRetries = 2, int currentRetry = 0)
    {
        if (currentRetry > countRetries)
        {
            return string.Empty;
        }

        await Task.Delay(_readMessagesTimeoutMs / countRetries, cancellationToken);

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

    public void SendCommand(CommandAwaiter command)
    {
        _commands.Push(command);
    }

    private async Task<SerialPort?> GetArduinoPort()
    {
        var serialPortNames = SerialPort.GetPortNames();

        foreach (var portname in serialPortNames)
        {
            var serialPort = new SerialPort(portname, _configuration.BaundRate);
            serialPort.DataBits = _configuration.DataBits;

            if(await CheckArduinoPort(serialPort))
            {
                return serialPort;
            }
        }

        return null;
    }

    private async Task<bool> CheckArduinoPort(SerialPort serialPort, int count = 5)
    {
        try
        {
            serialPort.ReadTimeout = 1000;
            serialPort.ErrorReceived += (object sender, SerialErrorReceivedEventArgs e) =>
            {
                throw new Exception("Some error with serial");
            };
           
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

        await Task.Delay(_sendMessagesIntervalMs);

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
}
