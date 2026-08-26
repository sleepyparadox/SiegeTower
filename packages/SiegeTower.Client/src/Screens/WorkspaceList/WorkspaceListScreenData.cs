using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListScreenData : IScreenData
{
	public GraphCache Cache { get; } = new();
	public SessionData Session { get; }
	public bool IsLoadedOnce { get; internal set; }
	public string Title => "Workspaces";
	public IReadOnlyList<WorkspaceRow> Workspaces { get; internal set; } = [];
	public WorkspaceListScreenSystem System { get; }
	public ToolbarGrid ToolbarGrid { get; }
	public Toolbar FileToolbar { get; }
	public Toolbar HelpToolbar { get; }
	public WorkspaceListDockContent WorkspaceListDockContent { get; }
	public WorkspaceListCreateContent WorkspaceListCreateContent { get; }
	public WorkspaceGitAuthContent WorkspaceGitAuthContent { get; }
	public DockGrid DockGrid { get; }

	public WorkspaceListScreenData(SessionData session)
	{
		Session = session ?? throw new ArgumentNullException(nameof(session));
		System = new();
		FileToolbar = new() { Name = "File", Items = [new("File", () => { }), new("Open", () => { }), new("Save", () => { })] };
		HelpToolbar = new() { Name = "Help", Items = [new("Help", () => { })] };
		ToolbarGrid = new() { Toolbars = [FileToolbar, HelpToolbar] };
		WorkspaceListDockContent = new(this);
		WorkspaceListCreateContent = new(this);
		WorkspaceGitAuthContent = new(this);
		DockGrid = new DockGrid(
			[WorkspaceListDockContent, WorkspaceGitAuthContent, new ColorDockContent { Name = "Red", Color = "Red" }, new ColorDockContent { Name = "Blue", Color = "Blue" }],
			[new ColorDockContent { Name = "Yellow", Color = "Yellow" }, new ColorDockContent { Name = "Green", Color = "Green" }],
			[WorkspaceListCreateContent, new ColorDockContent { Name = "Purple", Color = "Purple" }, new ColorDockContent { Name = "Orange", Color = "Orange" }]);
	}
}
