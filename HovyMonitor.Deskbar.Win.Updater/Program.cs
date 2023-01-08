using Octokit;
using System.IO.Compression;

var tempUpdateId = Guid.NewGuid().ToString().Replace("-", "");
var tempPath = Path.Combine(Path.GetTempPath(), tempUpdateId);

Console.WriteLine("Step 1 - Prepairings");


Console.WriteLine($"\tWelcome to updater! Your update Id is {tempUpdateId}");

Directory.CreateDirectory(tempPath);

Console.WriteLine($"\tTemp path: {tempPath}");

var github = new GitHubClient(new ProductHeaderValue("HovyMonitorBarUpdater"));

Console.WriteLine("\tUpdates is checking...");

var lastRelease = await github.Repository.Release.GetLatest("niklyadov", "hovy-monitor");

Console.WriteLine("\tFound a release:");
Console.WriteLine($"\t\tId\t{lastRelease.Id}");
Console.WriteLine($"\t\tAuthor\t{lastRelease.Author.Login}");
Console.WriteLine($"\t\tPub.\t{lastRelease.PublishedAt}");
Console.WriteLine($"\t\tTag\t{lastRelease.TagName}");
Console.WriteLine($"\t\tName\t{lastRelease.Name}");
Console.WriteLine($"\t\tDesc.\t{lastRelease.Body}");

Console.WriteLine($"\tFound an attachments ({lastRelease.Assets.Count}):");

foreach (var item in lastRelease.Assets)
    Console.WriteLine($"\t\t{item.Id} {item.Name} ({item.Size}) - {item.ContentType}");

Console.WriteLine("\tSeeking a file to download...");
var downloadAttach = lastRelease.Assets
    .Where(a => a.Name.Contains("hovymonitor_bar"))
    .Where(a => a.ContentType.Equals("application/x-zip-compressed"))
    .Single();

Console.WriteLine($"\tOk. Download link is {downloadAttach.BrowserDownloadUrl}");

Console.WriteLine("Step 2 - Downloading");

var downloadFileName = downloadAttach.Name;
Console.WriteLine($"\tDownloading a file into {downloadFileName}...");

using var client = new HttpClient();
using var s = await client.GetStreamAsync(downloadAttach.BrowserDownloadUrl);
using var fs = File.OpenWrite(Path.Combine(tempPath, downloadFileName));
await s.CopyToAsync(fs);
fs.Close();

Console.WriteLine("Step 3 - Working with artifacts");

var finalFilePath = Path.Combine(tempPath, downloadFileName);
var binariesDir = Path.Combine(tempPath, "bin");

Console.WriteLine($"\tUnzipping a file {finalFilePath}");
Directory.CreateDirectory(binariesDir);
ZipFile.ExtractToDirectory(finalFilePath, binariesDir);

var unInstallRunner = new CliRunner(Path.Combine(binariesDir, "uninstall_script.bat"), binariesDir);
unInstallRunner.MessageReceived += (string message) =>
{
    Console.WriteLine($"Runner -> \t{message}");
};
unInstallRunner.Run("", CancellationToken.None);

var installRunner = new CliRunner(Path.Combine(binariesDir, "install_script.bat"), binariesDir);
installRunner.MessageReceived += (string message) =>
{
    Console.WriteLine($"Runner -> \t{message}");
};
installRunner.Run("", CancellationToken.None);