using SiegeTower.Client.UX;
using SiegeTower.Data;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceGitAuthContent : IDockContent
{
	public string Name => "GitHub Access";

	public Dock? Parent { get; set; }

	public string AppId { get; set; } = string.Empty;

	public string InstallationId { get; set; } = string.Empty;

	public string PrivateKey { get; set; } = string.Empty;

	public GithubAccessToken? AccessToken { get; set; }

	public bool IsGenerating { get; set; }

	public Exception? Error { get; set; }
}