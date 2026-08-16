using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class WorkspaceFilesScreen : Screen
{
	private readonly Session session;

	public WorkspaceFilesScreen(Session session)
		: base("Workspace Files")
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
		WorkspaceFilesToolbar = new() { Name = "File", Items = ["File", "Open", "Save"] };
		ToolbarGrid = new()
		{
			Toolbars = [WorkspaceFilesToolbar]
		};
		FileTreeDockContent = new(this);
		FileEditDockContent = new();
		DockGrid = new DockGrid([FileTreeDockContent], [FileEditDockContent], []);
	}

	public DockGrid DockGrid { get; }

	public Toolbar WorkspaceFilesToolbar { get; }

	public ToolbarGrid ToolbarGrid { get; }

	public FileTreeDockContent FileTreeDockContent { get; }

	public FileEditDockContent FileEditDockContent { get; }

	public IDictionary<string, FileEditDockContent> OpenFiles { get; } = new Dictionary<string, FileEditDockContent>(StringComparer.Ordinal);

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		FileTreeDockContent.Files = await session.SessionServices.WorkspaceFileService.GetFiles(false, cancellationToken);
		session.Redraw();
	}

	public async Task OpenFileAsync(FileRow file)
	{
		ArgumentNullException.ThrowIfNull(file);
		if (!OpenFiles.TryGetValue(file.Path, out var content))
		{
			var files = await session.SessionServices.WorkspaceFileService.GetFiles(true);
			var fileWithContents = files.FirstOrDefault(item => string.Equals(item.Path, file.Path, StringComparison.Ordinal));
			content = new FileEditDockContent(fileWithContents ?? file);
			OpenFiles.Add(file.Path, content);
			DockService.Attach(DockGrid.Center, content);
		}

		DockGrid.Center.ActiveContent = content;
		session.Redraw();
	}
}