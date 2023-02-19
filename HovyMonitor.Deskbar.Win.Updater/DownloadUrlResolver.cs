using Octokit;

namespace HovyMonitor.Deskbar.Win.Updater
{
    internal interface IDownloadUrlResolver
    {
        public Task<Uri> GetDownloadUriAsync();
    }

    internal class GithubReleasesDownloadUrlResolver : IDownloadUrlResolver
    {
        private readonly GithubRepo _githubRepo;

        private Func<ReleaseAsset, bool>? _predicate;

        public GithubReleasesDownloadUrlResolver(GithubRepo githubRepo) 
        {
            _githubRepo = githubRepo;
        }

        public GithubReleasesDownloadUrlResolver WithPredicate(
            Func<ReleaseAsset, bool> predicate)
        {
            _predicate = predicate;
            return this;
        }

        public async Task<Uri> GetDownloadUriAsync()
        {
            var productHeader = new ProductHeaderValue("GithubReleasesDownloadUrlResolver");
            var github = new GitHubClient(productHeader);

            var lastRelease = await github.Repository.Release.GetLatest(_githubRepo.UserName,
                _githubRepo.RepositoryName);

            var assets = lastRelease.Assets.ToList();

            if (_predicate != null) 
                assets = assets.Where(_predicate).ToList();

            var downloadAttach = assets.Single();

            return new Uri(downloadAttach.BrowserDownloadUrl);
        }
    }

    internal class GithubRepo
    {
        public string UserName { get; init; }
        public string RepositoryName { get; init; }

        public GithubRepo(string userName, string repositoryName)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));
            if (string.IsNullOrEmpty(repositoryName)) throw new ArgumentNullException(nameof(repositoryName));

            UserName = userName;
            RepositoryName = repositoryName;
        }

        public static GithubRepo FromString(string githubUsernameAndRepo)
        {
            var temp = githubUsernameAndRepo.Trim().Split('/');

            if (temp.Length != 2) throw new InvalidOperationException($"Param {nameof(githubUsernameAndRepo)} is invalid! " +
                $"Should be formatted as username/reponame.");

            return new GithubRepo(temp[0].Trim(), temp[1].Trim());
        }
    }
}
