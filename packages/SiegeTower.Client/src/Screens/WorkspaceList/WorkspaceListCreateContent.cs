using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListCreateContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Create";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public WorkspaceListCreateContent(WorkspaceListScreenData data)
	{
		Data = data ?? throw new ArgumentNullException(nameof(data));
	}

	public WorkspaceListScreenData Data { get; }

	public string WorkspaceName { get; set; } = "";

	public bool IsCreating { get; set; }

}
