using SiegeTower.Client.UX;
using SiegeTower.Data;

namespace SiegeTower.Client.Screens.WorkspaceGit;

public sealed class WorkspaceProjectListContent : IDockContent
{
	private readonly WorkspaceGitScreen screen;

	public WorkspaceProjectListContent(WorkspaceGitScreen screen)
	{
		this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
	}

	string IDockContent.Name => "Projects";

	Dock? IDockContent.Parent { get; set; }

	public IReadOnlyList<WorkspaceProjectRow> Projects { get; set; } = [];

	public Task PullProjectAsync(string @namespace) => screen.PullProjectAsync(@namespace);
}