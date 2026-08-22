using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceGit;

public sealed class WorkspaceProjectAddContent : IDockContent
{
	private readonly WorkspaceGitScreen screen;

	public WorkspaceProjectAddContent(WorkspaceGitScreen screen)
	{
		this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
	}

	string IDockContent.Name => "Add Project";

	Dock? IDockContent.Parent { get; set; }

	public string Namespace { get; set; } = string.Empty;

	public string GitRepo { get; set; } = string.Empty;

	public bool IsAdding { get; set; }

	public Exception? Error { get; set; }

	public Task AddAsync() => screen.AddProjectAsync();
}