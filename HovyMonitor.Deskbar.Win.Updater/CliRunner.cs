using System.Diagnostics;

public delegate void MessageReceived(string message);

public class CliRunner
{
    public event MessageReceived? MessageReceived;

    private readonly String _fileName;
    private readonly String _workPath;

    public CliRunner(String fileName, String workPath)
    {
        _fileName = fileName;
        _workPath = workPath;
    }

    public Process Run(String command, CancellationToken cancellation)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _fileName,
            Arguments = command,
            UseShellExecute = false,
            WorkingDirectory = _workPath
        };

        var process = new Process
        {
            StartInfo = startInfo
        };

        var messagesAwaiterTask = Task.Run(async () => await ReceiveMessages(process, cancellation), cancellation);

        process.Start();

        process.WaitForExit();

        messagesAwaiterTask.Dispose();

        return process;
    }

    private async Task ReceiveMessages(Process process, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            var output = await process.StandardOutput.ReadLineAsync();

            if (string.IsNullOrEmpty(output)) continue;

            MessageReceived?.Invoke(output);

            await Task.Delay(100, cancellation);
        }
    }
}