using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceGit;

public sealed class WorkspaceGitAuthContent : IDockContent
{
	public string Name => "GitHub Access";

	public Dock? Parent { get; set; }

	public string AppId { get; set; } = string.Empty;

	public string InstallationId { get; set; } = string.Empty;

	public string PrivateKey { get; set; } = string.Empty;

	public bool IsGenerating { get; set; }

	public Exception? Error { get; set; }
}