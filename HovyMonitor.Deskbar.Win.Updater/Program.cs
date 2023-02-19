using HovyMonitor.Deskbar.Win.Updater;
using HovyMonitor.Deskbar.Win.Updater.Extensions;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

internal class Program
{
    private static Mutex? _mutex;
    private const string APP_NAME = "HovyMonitor.Deskbar.Win.Updater.exe";

    public static readonly ArgumentOptions OptionsArgs = new ArgumentOptions();
    public static string Identifier =  $"{Guid.NewGuid()}".Replace("-", "");
    public static string InstancePath = "C:\\Program Files\\HovyMonitorBar";
    public static string DefaultRepoName = "niklyadov/hovy-monitor";
    public static bool IsLaunchingFromProgramDirectory
        => Environment.CurrentDirectory == InstancePath;

    public static bool IsLaunchingFromTempDirectory
        => Environment.CurrentDirectory.StartsWith(Path.GetTempPath());
    public static string TemporaryPath
        => Path.Combine(Path.GetTempPath(), Identifier);

    private static async Task Main(string[] args)
    {
        Console.BackgroundColor = ConsoleColor.DarkMagenta;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Clear();
        Console.Beep(1000, 50);

        ParseCommandLineArgs(args);

        while (!CheckForOneInstanceRunning() && OptionsArgs.Step == 0)
        {
            Thread.Sleep(100);

            Console.WriteLine($"{APP_NAME} is already running!");

            if (OptionsArgs.WaitForWhileSecondInstanceDie)
                break;

            Console.WriteLine($"{APP_NAME} is already running! Exiting the application.");
            Console.ReadKey();

            Environment.Exit(0);
            return;
        }

        await ChooseInstallWayAsync();
    }

    private static bool CheckForOneInstanceRunning()
    {
        _mutex = new Mutex(true, APP_NAME, out bool createdNew);

        return createdNew;
    }

    private static void ParseCommandLineArgs(string[] args)
    {
        var parser = new CommandLineParser.CommandLineParser();
        parser.ExtractArgumentAttributes(OptionsArgs);
        parser.ParseCommandLine(args);

        if (!string.IsNullOrEmpty(OptionsArgs.InternalId))
            Identifier = OptionsArgs.InternalId;

        if (!string.IsNullOrEmpty(OptionsArgs.InstancePath))
            InstancePath = OptionsArgs.InstancePath;

        if (!string.IsNullOrEmpty(OptionsArgs.RepositoryName))
            DefaultRepoName = OptionsArgs.RepositoryName;

        Console.WriteLine(OptionsArgs);
    }

    private static async Task ChooseInstallWayAsync()
    {
        await DoInstallAsync();

        Console.WriteLine($"After install");
    }


    private static async Task DoInstallAsync()
    {
        Console.WriteLine($"Step: {OptionsArgs.Step}");

        if (OptionsArgs.Step == 1)
        {
            Console.WriteLine($"Step: InstallScript");

            UnnstallScript();
            InstallScript();

            Environment.Exit(0);
            return;
        }

        if (OptionsArgs.Step == 2)
        {
            Console.WriteLine($"Step: UnnstallScript");

            UnnstallScript();

            Environment.Exit(0);
            return;
        }


        if (IsLaunchingFromTempDirectory)
        {
            Console.WriteLine($"From temp: {IsLaunchingFromTempDirectory}");

            UnnstallScript();

            GenerateHelperScriptAsync();

            Environment.Exit(0); 
            return;
        }

        var uriResolver = new GithubReleasesDownloadUrlResolver(
            GithubRepo.FromString(DefaultRepoName));

        var downloadLink = await uriResolver
            .WithPredicate(a => a.ContentType.Equals("application/x-zip-compressed"))
            .GetDownloadUriAsync();

        var downloader = new HttpDownloader(downloadLink);

        var downloadedFilePath = Path.Combine(TemporaryPath, "artifacts.zip");

        await downloader.SaveFileTo(downloadedFilePath);

        downloadedFilePath = "C:\\Users\\Nick\\Desktop\\i.zip";

        Console.WriteLine($"Zip: {downloadedFilePath}");

        ZipFile.ExtractToDirectory(downloadedFilePath, TemporaryPath);

        var updaterPath = Path.Combine(TemporaryPath, "HovyMonitor.Deskbar.Win.Updater.exe");

        Console.WriteLine($"Updater: {updaterPath}");


        new CliRunner(updaterPath, TemporaryPath)
            .Run($"-w -i {Identifier}", CancellationToken.None);

        Environment.Exit(0);
        return;
    }

    private static void GenerateHelperScriptAsync()
    {
        var sb = new StringBuilder();

        sb.AppendLine("@ECHO OFF");
        sb.AppendLine($"@cd /d \"{TemporaryPath}\"");
        sb.AppendLine("@setlocal enableextensions");
        sb.AppendLine($"title Helper Script");
        sb.AppendLine("net session >nul 2>&1");
        sb.AppendLine("if %errorLevel% == 0 (");
        sb.AppendLine("\techo Administrative permissions confirmed.");
        sb.AppendLine(") else (");
        sb.AppendLine("\techo Please run this script with administrator permissions.");
        sb.AppendLine("\tpause");
        sb.AppendLine("\texit /b 0");
        sb.AppendLine(")");
        sb.AppendLine(":LOOP");
        sb.AppendLine("PSLIST HovyMonitor.Deskbar.Win.Updater >nul 2>&1");
        sb.AppendLine("IF ERRORLEVEL 1 (");
        sb.AppendLine("\tGOTO CONTINUE");
        sb.AppendLine(") ELSE (");
        sb.AppendLine("\tECHO is still running");
        sb.AppendLine("\tTIMEOUT / T 1 ");
        sb.AppendLine("\tGOTO LOOP ");
        sb.AppendLine(")");
        sb.AppendLine(":CONTINUE");

        sb.AppendLine($"@RD /S /Q \"{InstancePath}\"");
        sb.AppendLine($"MKDIR \"{InstancePath}\"");

        sb.AppendLine($"robocopy \"{TemporaryPath}\" \"{InstancePath}\" /MOV /XF script-helper.bat /is /mt /nc /ns /njh /njs");

        sb.AppendLine($"cd /D \"{InstancePath}\"");
        sb.AppendLine($"start HovyMonitor.Deskbar.Win.Updater.exe -w -i {Identifier} -s 1");
        sb.AppendLine($"@RD /S /Q \"{TemporaryPath}\"");
        sb.AppendLine("exit /b 0");

        var helperScriptFileName = "script-helper.bat";
        var helperScriptFullPath = Path.Combine(TemporaryPath, helperScriptFileName);
        File.WriteAllText(helperScriptFullPath, sb.ToString());

        Console.WriteLine($"Helper: {helperScriptFullPath}");

        new CliRunner("CMD", TemporaryPath)
                .Run($"/C {helperScriptFullPath}", CancellationToken.None);
    }

    private static void InstallScript()
    {
        Console.WriteLine($"InstallScript");

        var regasmPath = ResolveRegasmPathes().Last();

        Console.WriteLine($"regasmPath = {regasmPath}");

        if (string.IsNullOrEmpty(regasmPath))
            throw new InvalidOperationException("RegASM path is not resolved properly.");

        new CliRunner(regasmPath, TemporaryPath)
                .Run($"/nologo /codebase \"{Path.Combine(InstancePath, "HovyMonitor.DeskBar.Win.dll")}\"", CancellationToken.None);

        new CliRunner("taskkill", "/")
                .Run("/im explorer.exe /f", CancellationToken.None);

        Process.Start(Path.Combine(Environment.GetEnvironmentVariable("windir")!, "explorer.exe"));
    }

    private static void UnnstallScript()
    {
        Console.WriteLine($"UnnstallScript");

        var regasmPath = ResolveRegasmPathes().Last();

        Console.WriteLine($"regasmPath = {regasmPath}");

        if (string.IsNullOrEmpty(regasmPath))
            throw new InvalidOperationException("RegASM path is not resolved properly.");

        new CliRunner(regasmPath, TemporaryPath)
                .Run($"/nologo /u \"{Path.Combine(InstancePath, "HovyMonitor.DeskBar.Win.dll")}\"", CancellationToken.None);

        new CliRunner("taskkill", "/")
            .Run("/im explorer.exe /f", CancellationToken.None);

        Process.Start(Path.Combine(Environment.GetEnvironmentVariable("windir")!, "explorer.exe"));
    }

    private static string[] ResolveRegasmPathes()
    {
        var dotnetPathes = new List<string>();

        var basePath = Path.Combine("/Windows", "Microsoft.NET");

        foreach (var dotnetDir in Directory.GetDirectories(basePath))
        foreach (var dotnetVer in Directory.GetDirectories(dotnetDir))
        {
                var regasm = Directory.EnumerateFiles(dotnetVer)
                        .Where(x => Path.GetFileName(x).EqualsIgnoreCase("regasm.exe"))
                        .FirstOrDefault();

                if (!string.IsNullOrEmpty(regasm))
                    dotnetPathes.Add(regasm);
        }

        return dotnetPathes.ToArray();
    }
}


internal class HttpDownloader {

    private readonly HttpClient _httpClient
        = new HttpClient();

    private readonly Uri _downloadUrl;

    public HttpDownloader(Uri downloadFrom)
    {
        _downloadUrl = downloadFrom;
    }

    public async Task<string> SaveFileTo(string downloadTo)
    {
        if (string.IsNullOrEmpty(downloadTo))
            throw new ArgumentNullException(nameof(downloadTo));

        var workindDirectory = Path.GetDirectoryName(downloadTo);
        Directory.CreateDirectory(workindDirectory!);

        using var s = await _httpClient.GetStreamAsync(_downloadUrl);
        using var fs = File.Create(downloadTo);

        await s.CopyToAsync(fs);

        fs.Close();

        return downloadTo;
    }
}