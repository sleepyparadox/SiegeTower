using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListDockContent : IDockContent
{
	private readonly WorkspaceListScreenData data;

	public WorkspaceListDockContent(WorkspaceListScreenData data)
	{
		this.data = data ?? throw new ArgumentNullException(nameof(data));
	}

	#region  IDockContent

	string IDockContent.Name { get => "Workspaces"; }

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public IReadOnlyList<WorkspaceRow> Workspaces { get; set; } = [];

	public void OpenWorkspace(string id) => data.System.OpenWorkspace(data, id);

	public Task DeleteWorkspaceAsync(string id) => data.System.DeleteWorkspaceAsync(data, id);

	public Task DeleteAllWorkspacesAsync() => data.System.DeleteAllWorkspacesAsync(data);
}
