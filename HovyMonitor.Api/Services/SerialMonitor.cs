using System.IO.Ports;
using HovyMonitor.Api.Entity;
using HovyMonitor.Api.Workers;
using Microsoft.Extensions.Options;

namespace HovyMonitor.Api.Services;

public class SerialMonitor
{
    private readonly Queue<CommandAwaiter> _commands = new();
    private readonly SerialPortConfiguration _configuration;
    private readonly ILogger<SerialMonitor> _logger;
    private DateTime? _lastCommandExecutionTime = null;
    private Task? _commandHandleTask = null;

    private SerialPortInputOutput? _port;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource? _commandHandleTaskCts;
    
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
            if (_commandHandleTask == null || _lastCommandExecutionTime == null)
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
        if (_port == null || !_port.IsOpen)
        {
            _port = await GetArduinoPort();
        }

        _logger.LogDebug("Starting a Commands listener");

        _commandHandleTaskCts = new CancellationTokenSource();
        _commandHandleTask = Task.Run(HandleCommands, _commandHandleTaskCts.Token);
    }
    
    private async Task RestartHandleCommands()
    {
        _port?.CloseConnection();

        _commandHandleTaskCts?.Cancel();
        _commandHandleTask?.Dispose();
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
                    
            _port?.SendUbcCommand(command.CommandName);
            _logger.LogDebug("Success send command {CommandName} to board", command.CommandName);
                    
            command.CommandResponse = WaitCommandResponse(command.CommandName);
                    
            _logger.LogDebug("Success receive a data for command {CommandName}", command.CommandName);
                    
            _lastCommandExecutionTime = DateTime.UtcNow;
        }
    }

    private string WaitCommandResponse(string commandName, int countRetries = 2, int currentRetry = 1)
    {
        _logger.LogDebug("Wait command {CommandName} response, retry: ({Retry}/{TotalRetries})", commandName, currentRetry, countRetries);

        if (currentRetry >= countRetries)
        {
            return string.Empty;
        }

        Thread.Sleep(_configuration.ReadTimeout / countRetries);

        if (_port == null)
        {
            throw new Exception("Port is null");
        }
        
        if (!_port.IsOpen)
        {
            throw new Exception("Serial port is closed");
        }

        const string endingSymbols = ";\r\n";
        string responseString = "";
        int responseReadingCount = 0;
        const int maxResponseReadingCount = 10;

        // ожидаем конечный символ, но не более 10 раз
        while (!responseString.EndsWith(endingSymbols) && responseReadingCount <= maxResponseReadingCount)
        {
            responseReadingCount++;
            responseString += _port.PopBuffer();
            Thread.Sleep(250);
            
            _logger.LogDebug("Waiting the ending symbol of command {CommandName}. Retry: {Retry} of {MaxReries}", commandName, responseReadingCount, maxResponseReadingCount);
        }

        // если ждем уже очень долго (сделали более maxResponseReadingCount попыток), возьмем первое попавшееся значение
        if (responseReadingCount >= maxResponseReadingCount && responseString.Contains("\r\n"))
        {
            responseString = responseString.Split("\r\n").First();
            _logger.LogWarning("Waiting for {CommandName} is too long... Getting first response string {ResponseString}", commandName, responseString);
        }

        if (!string.IsNullOrEmpty(responseString))
        {
            return responseString;
        }

        return WaitCommandResponse(commandName, countRetries, currentRetry + 1);
    }

    private async Task<SerialPortInputOutput> GetArduinoPort()
    {
        var serialPortNames = SerialPort.GetPortNames();
        _logger.LogDebug("Search an arduino port. Total ports count {PortsCount}", serialPortNames.Length);
        foreach (var portName in serialPortNames)
        {
            var serialPortConfiguration =
                new SerialPortInputOutputConfig(portName, _configuration.BandRate, _configuration.DataBits);

            var serialPort = new SerialPortInputOutput(serialPortConfiguration);

            _logger.LogDebug("Created new serial port connection. {Config}", serialPortConfiguration);
            
            if(await CheckArduinoPort(serialPort))
            {
                _logger.LogInformation("Port with name {PortName} is a arduino port", portName);
                return serialPort;
            }
            
            _logger.LogInformation("Port with name {PortName} is a not arduino port", portName);
        }

        await Task.Delay(500);
        return await GetArduinoPort();
    }

    private async Task<bool> CheckArduinoPort(SerialPortInputOutput serialPort, int count = 5)
    {
        try
        {
            serialPort.OpenConnection();
            
            Thread.Sleep(3000);

            if (count <= 0)
            {
                throw new Exception("Time out");
            }

            if (await PingArduinoBoard(serialPort))
            {
                return true;
            }

            return await CheckArduinoPort(serialPort, count - 1);
        }
        catch(Exception ex)
        {
            _logger.LogError("Check arduino port failed with error {Exception}", ex);
            serialPort.CloseConnection();
            return false;
        }
    }

    private async Task<bool> PingArduinoBoard(SerialPortInputOutput serialPort)
    {

        const string ping = "ping";
        const string pong = "pong";

        serialPort.SendUbcCommand(ping);
        await Task.Delay(_configuration.SendInterval, _cancellationToken);

        const int maxCheckRetries = 10;
        for (int i = 1; i <= 10; i++)
        {
            _logger.LogInformation("[ping] {N} of {TotalN}", i, maxCheckRetries);
            
            if (serialPort.PeekBuffer().Trim() != string.Empty)
            {
                var response = serialPort.PopBuffer().Trim();
                if (response.Equals(pong, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[ping] Success ping. This port is a Arduino Port! {Config}", serialPort.Config);
                    return true;
                }
                
                _logger.LogInformation("[ping] Check {N}/{TotalN}. '{Response}' != '{ResponseNeeded}'", i, maxCheckRetries, response, pong);
            }

            _logger.LogWarning("[ping] Failed ping. {Config}", serialPort.Config);
        }

        return false;
    }
    #endregion
}
