using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class WorkspaceHomeScreen : Screen
{
	readonly Session session;

	public WorkspaceHomeScreen(Session session)
		: base("Workspace")
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
		WorkspaceToolbar = new()
		{
			Name = "Workspace",
			Items =
			[
				new("Workspace", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceScreen(session.SessionContext.WorkspaceID))),
				new("Git", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceGitScreen(session.SessionContext.WorkspaceID))),
				new("Files", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceFilesScreen(session.SessionContext.WorkspaceID)))
			]
		};
		ToolbarGrid = new() { Toolbars = [WorkspaceToolbar] };
	}

	public Toolbar WorkspaceToolbar { get; }

	public ToolbarGrid ToolbarGrid { get; }
}