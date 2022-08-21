namespace HovyMonitor.Api.Workers;

public delegate void CommandResponseReceived(string message);

public class CommandAwaiter {
    public string CommandName { get; set; }

    private string _commandResponse = string.Empty;
    public string CommandResponse {
        get => _commandResponse;
        set
        {
            _commandResponse = value;

            if(CommandResponseReceived != null)
                CommandResponseReceived(_commandResponse);
        }
    }

    public event CommandResponseReceived? CommandResponseReceived;

    public CommandAwaiter(string commandName)
    {
        CommandName = commandName;
    }
}