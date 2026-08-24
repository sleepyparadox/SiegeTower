using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class WorkspaceFilesScreen : Screen
{
	private readonly GraphCache _unitOfWork = new();
	private readonly Session session;

	public WorkspaceFilesScreen(Session session)
		: base("Workspace Files")
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
		LoadingQueue.Changed += HandleLoadingQueueChanged;
		WorkspaceToolbar = new()
		{
			Name = "Workspace",
			Items =
			[
				new("Workspace", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceScreen(session.SessionContext.WorkspaceID))),
				new("Files", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceFilesScreen(session.SessionContext.WorkspaceID)))
			]
		};
		ToolbarGrid = new()
		{
			Toolbars = [WorkspaceToolbar]
		};
		FileTreeDockContent = new(this);
		FileEditDockContent = new();
		DockGrid = new DockGrid([FileTreeDockContent], [FileEditDockContent], []);
	}

	public DockGrid DockGrid { get; }

	public Toolbar WorkspaceToolbar { get; }

	public ToolbarGrid ToolbarGrid { get; }

	public FileTreeDockContent FileTreeDockContent { get; }

	public FileEditDockContent FileEditDockContent { get; }

	public SessionServices SessionServices => session.SessionServices;

	public SessionContext SessionContext => session.SessionContext;

	internal GraphCache UnitOfWork => _unitOfWork;

	public IDictionary<string, FileEditDockContent> OpenFiles { get; } = new Dictionary<string, FileEditDockContent>(StringComparer.Ordinal);

	public void Redraw() => session.Redraw();

	public override Task Load() => LoadAsync();

	public Task LoadAsync(CancellationToken cancellationToken = default) => TrackAsync(LoadCoreAsync(cancellationToken));

	private async Task LoadCoreAsync(CancellationToken cancellationToken)
	{
		FileTreeDockContent.Files = await WorkspaceFileService.GetFiles(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, false, cancellationToken);
		session.Redraw();
	}

	public Task OpenFileAsync(FileRow file) => TrackAsync(OpenFileCoreAsync(file));

	private async Task OpenFileCoreAsync(FileRow file)
	{
		ArgumentNullException.ThrowIfNull(file);
		if (!OpenFiles.TryGetValue(file.Path, out var content))
		{
			var files = await WorkspaceFileService.GetFiles(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, true);
			var fileWithContents = files.FirstOrDefault(item => string.Equals(item.Path, file.Path, StringComparison.Ordinal));
			content = new FileEditDockContent(fileWithContents ?? file);
			OpenFiles.Add(file.Path, content);
			DockService.Attach(DockGrid.Center, content);
		}

		DockGrid.Center.ActiveContent = content;
		session.Redraw();
	}

	private async Task TrackAsync(Task task)
	{
		LoadingQueue.Append(task);
		await task;
	}

	private void HandleLoadingQueueChanged(object? sender, EventArgs args) => session.Redraw();
}