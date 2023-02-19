using CommandLineParser.Arguments;

namespace HovyMonitor.Deskbar.Win.Updater
{
    internal record ArgumentOptions
    {
        [ValueArgument(typeof(string), 'i',
            Description = "Set a local updater identifier of update")]
        public string InternalId { get; set; } = string.Empty;

        [ValueArgument(typeof(string), 'p',
            Description = "Set a local instance path")]
        public string InstancePath { get; set; } = string.Empty;

        [ValueArgument(typeof(string), 'r',
            Description = "Set a repository with updates. With format (username/repo_name")]
        public string RepositoryName { get; set; } = string.Empty;

        [SwitchArgument('w', false)]
        public bool WaitForWhileSecondInstanceDie { get; set; } = false;

        [ValueArgument(typeof(int), 's',
           Description = "Set a installation step")]
        public int Step { get; set; } = 0;
    }
}
