using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListDockContent : IDockContent
{
	#region  IDockContent

	string IDockContent.Name { get => "Workspaces"; }

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public IReadOnlyList<WorkspaceRow> Workspaces { get; set; } = [];
}
