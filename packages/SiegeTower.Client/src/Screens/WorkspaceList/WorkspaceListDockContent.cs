using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListDockContent : IDockContent
{
	private readonly WorkspaceListScreen screen;

	public WorkspaceListDockContent(WorkspaceListScreen screen)
	{
		this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
	}

	#region  IDockContent

	string IDockContent.Name { get => "Workspaces"; }

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public IReadOnlyList<WorkspaceRow> Workspaces { get; set; } = [];

	public void OpenWorkspace(string id) => screen.OpenWorkspace(id);

	public Task DeleteWorkspaceAsync(string id) => screen.DeleteWorkspaceAsync(id);
}
