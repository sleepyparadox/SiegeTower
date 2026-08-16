using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListCreateContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Create";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public WorkspaceListCreateContent(WorkspaceListScreen screen)
	{
		Screen = screen ?? throw new ArgumentNullException(nameof(screen));
	}

	public WorkspaceListScreen Screen { get; }

	public string WorkspaceName { get; set; } = "";

	public bool IsCreating { get; set; }

}
