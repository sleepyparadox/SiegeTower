using SiegeTower.Data;

namespace SiegeTower.Client.Screens.WorkspaceGit;

public sealed class WorkspaceGitAuthContent
{
	public string AppId { get; set; } = string.Empty;

	public string InstallationId { get; set; } = string.Empty;

	public string PrivateKey { get; set; } = string.Empty;

	public bool IsGenerating { get; set; }

	public Exception? Error { get; set; }
}