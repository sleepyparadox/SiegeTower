using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Screens.Ollama;
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
		ToolbarGrid = new()
		{
			Toolbars = [WorkspaceToolbar]
		};
		FileTreeDockContent = new(this);
		FileEditDockContent = new();
		ChatPrimaryContent = new() { WorkspaceFilesScreen = this };
		DockGrid = new DockGrid([FileTreeDockContent], [FileEditDockContent], [ChatPrimaryContent]);
	}

	public DockGrid DockGrid { get; }

	public Toolbar WorkspaceToolbar { get; }

	public ToolbarGrid ToolbarGrid { get; }

	public FileTreeDockContent FileTreeDockContent { get; }

	public FileEditDockContent FileEditDockContent { get; }

	public ChatPrimaryContent ChatPrimaryContent { get; }

	public SessionServices SessionServices => session.SessionServices;

	public SessionContext SessionContext => session.SessionContext;

	public IDictionary<string, FileEditDockContent> OpenFiles { get; } = new Dictionary<string, FileEditDockContent>(StringComparer.Ordinal);

	public void Redraw() => session.Redraw();

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		FileTreeDockContent.Files = await session.SessionServices.WorkspaceFileService.GetFiles(false, cancellationToken);
		var workspaceID = session.SessionContext.WorkspaceID;
		if (!string.IsNullOrWhiteSpace(workspaceID))
		{
			ChatPrimaryContent.WorkspaceHistory.Clear();
			ChatPrimaryContent.WorkspaceHistory.AddRange(await session.SessionServices.OllamaService.ChatWorkspace(workspaceID, cancellationToken));
		}
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