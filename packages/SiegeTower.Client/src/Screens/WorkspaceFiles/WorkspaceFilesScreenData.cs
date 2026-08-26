using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class WorkspaceFilesScreenData : IScreenData
{
	public GraphCache Cache { get; } = new();
	public SessionData Session { get; }
	public LoadingQueue LoadingQueue { get; } = new();
	public string Title => "Workspace Files";
	public IDictionary<string, FileEditDockContent> OpenFiles { get; } = new Dictionary<string, FileEditDockContent>(StringComparer.Ordinal);
	public WorkspaceFilesScreenSystem System { get; }
	public Toolbar WorkspaceToolbar { get; }
	public ToolbarGrid ToolbarGrid { get; }
	public FileTreeDockContent FileTreeDockContent { get; }
	public FileEditDockContent FileEditDockContent { get; }
	public DockGrid DockGrid { get; }

	public WorkspaceFilesScreenData(SessionData session)
	{
		Session = session ?? throw new ArgumentNullException(nameof(session));
		System = new();
		WorkspaceToolbar = new() { Name = "Workspace", Items = [new("Workspace", () => Session.NavigateTo($"workspace/{Session.Context.WorkspaceID}")), new("Files", () => Session.NavigateTo($"workspace/{Session.Context.WorkspaceID}/files"))] };
		ToolbarGrid = new() { Toolbars = [WorkspaceToolbar] };
		FileTreeDockContent = new(this);
		FileEditDockContent = new();
		DockGrid = new DockGrid([FileTreeDockContent], [FileEditDockContent], []);
	}
}
