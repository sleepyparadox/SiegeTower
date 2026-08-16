using SiegeTower.Data;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.API;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListScreen : Screen
{
	private readonly Session session;

	public WorkspaceListScreen(Session session)
		: base("Workspaces")
	{
		ArgumentNullException.ThrowIfNull(session);
		FileToolbar = new() { Name = "File", Items = ["File", "Open", "Save"] };
		HelpToolbar = new() { Name = "Help", Items = ["Help"] };
		ToolbarGrid = new()
		{
			Toolbars = [FileToolbar, HelpToolbar]
		};
		this.session = session;
		WorkspaceListDockContent = new();
		WorkspaceListCreateContent = new(session.SessionContext);
		DockGrid = new DockGrid(
			[
				WorkspaceListDockContent,
				new ColorDockContent { Name = "Red", Color = "Red" },
				new ColorDockContent { Name = "Blue", Color = "Blue" }
			],
			[
				new ColorDockContent { Name = "Yellow", Color = "Yellow" },
				new ColorDockContent { Name = "Green", Color = "Green" }
			],
			[
				WorkspaceListCreateContent,
				new ColorDockContent { Name = "Purple", Color = "Purple" },
				new ColorDockContent { Name = "Orange", Color = "Orange" }
			]);
		session.SetActiveScreen(this);
	}

	public IReadOnlyList<WorkspaceRow> Workspaces { get; private set; } = [];

	public ToolbarGrid ToolbarGrid { get; }

	public Toolbar FileToolbar { get; }

	public Toolbar HelpToolbar { get; }

	public WorkspaceListDockContent WorkspaceListDockContent { get; }

	public WorkspaceListCreateContent WorkspaceListCreateContent { get; }

	public DockGrid DockGrid { get; }

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		var workspaces = await APIService.Get<WorkspaceRow>(session.SessionContext, cancellationToken);
		Workspaces = workspaces;
		WorkspaceListDockContent.Workspaces = workspaces;
		session.Redraw();
	}
}