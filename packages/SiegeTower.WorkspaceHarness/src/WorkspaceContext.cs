using SiegeTower.Data.Ollama;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceContext
{
	private readonly object sync = new();
	private readonly List<WorkspaceProjectRow> projects = [];

	public WorkspaceContext(IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		Services = new WorkspaceServices(configuration, httpClientFactory);
	}

	public WorkspaceServices Services { get; }

	public TimeSpan PromptTimeout { get; } = TimeSpan.FromMinutes(5);

	public WorkspaceSettings Settings { get; } = new();

	public void UpdateSettings(WorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);
		lock (sync)
		{
			Settings.GitAccessToken = settings.GitAccessToken;
			Settings.GitBranchName = settings.GitBranchName;
			Settings.GitPR = settings.GitPR;
		}
	}

	public IReadOnlyList<WorkspaceProjectRow> GetProjects()
	{
		lock (sync)
		{
			return projects.ToArray();
		}
	}

	public WorkspaceProjectRow AddProject(WorkspaceProjectRow project)
	{
		ArgumentNullException.ThrowIfNull(project);
		ArgumentException.ThrowIfNullOrWhiteSpace(project.Namespace);
		ArgumentException.ThrowIfNullOrWhiteSpace(project.GitRepo);
		lock (sync)
		{
			projects.RemoveAll(item => string.Equals(item.Namespace, project.Namespace, StringComparison.Ordinal));
			projects.Add(project);
			return project;
		}
	}

	public async Task PullProjectAsync(string projectNamespace, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(projectNamespace);
		WorkspaceProjectRow project;
		string? accessToken;
		lock (sync)
		{
			project = projects.FirstOrDefault(item => string.Equals(item.Namespace, projectNamespace, StringComparison.Ordinal))
				?? throw new KeyNotFoundException($"Unknown workspace project '{projectNamespace}'.");
			accessToken = Settings.GitAccessToken;
		}
		if (string.IsNullOrWhiteSpace(accessToken))
		{
			throw new InvalidOperationException("A GitHub access token is required to pull a project.");
		}
		await Services.GitService.PullAsync(project, accessToken, cancellationToken);
	}

	public GitStatus GetGitStatus()
	{
		lock (sync)
		{
			return new GitStatus
			{
				GithubAccessTokenExists = !string.IsNullOrWhiteSpace(Settings.GitAccessToken)
			};
		}
	}

	public void SetGithubAccessToken(string token, DateTime expiresAtUtc)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(token);
		lock (sync)
		{
			Settings.GitAccessToken = token;
		}
	}
		public sealed class WorkspaceSettings : Data.WorkspaceSettings
		{
		}
}