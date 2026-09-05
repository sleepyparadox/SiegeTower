using SiegeTower.Data.Ollama;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceContext
{
	private readonly object sync = new();

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

	public string? GetGitAccessToken()
	{
		lock (sync)
		{
			return Settings.GitAccessToken;
		}
	}

		public sealed class WorkspaceSettings : Data.WorkspaceSettings
		{
		}
}