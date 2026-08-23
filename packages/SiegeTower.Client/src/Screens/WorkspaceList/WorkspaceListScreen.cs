using SiegeTower.Data;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.API;
using SiegeTower.Client.UX;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListScreen : Screen
{
	private readonly GraphCache _unitOfWork = new();
	private readonly Session session;

	public WorkspaceListScreen(Session session)
		: base("Workspaces")
	{
		ArgumentNullException.ThrowIfNull(session);
		LoadingQueue.Changed += HandleLoadingQueueChanged;
		FileToolbar = new() { Name = "File", Items = [new("File", () => { }), new("Open", () => { }), new("Save", () => { })] };
		HelpToolbar = new() { Name = "Help", Items = [new("Help", () => { })] };
		ToolbarGrid = new()
		{
			Toolbars = [FileToolbar, HelpToolbar]
		};
		this.session = session;
		WorkspaceListDockContent = new(this);
		WorkspaceListCreateContent = new(this);
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

	public override Task Load() => LoadAsync();

	public Task LoadAsync(CancellationToken cancellationToken = default) => TrackAsync(LoadCoreAsync(cancellationToken));

	private async Task LoadCoreAsync(CancellationToken cancellationToken)
	{
		var workspaces = await APIService.Get<WorkspaceRow>(_unitOfWork, session.SessionContext, cancellationToken);
		Workspaces = workspaces;
		WorkspaceListDockContent.Workspaces = workspaces;
		session.Redraw();
	}

	public void OpenWorkspace(string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		session.SessionContext.WorkspaceID = id;
		session.NavigateTo(session.GetNavigationUrlToWorkspaceScreen(id));
	}

	public Task CreateAsync(CancellationToken cancellationToken = default) => TrackAsync(CreateCoreAsync(cancellationToken));

	private async Task CreateCoreAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(WorkspaceListCreateContent.WorkspaceName) || WorkspaceListCreateContent.IsCreating)
		{
			return;
		}

		WorkspaceListCreateContent.IsCreating = true;
		try
		{
			await APIService.CreateWorkspace(_unitOfWork, session.SessionContext, WorkspaceListCreateContent.WorkspaceName.Trim(), cancellationToken);
			WorkspaceListCreateContent.WorkspaceName = string.Empty;
			await LoadAsync(cancellationToken);
		}
		finally
		{
			WorkspaceListCreateContent.IsCreating = false;
		}
	}

	public Task DeleteWorkspaceAsync(string id, CancellationToken cancellationToken = default) => TrackAsync(DeleteWorkspaceCoreAsync(id, cancellationToken));

	private async Task DeleteWorkspaceCoreAsync(string id, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		await APIService.DeleteWorkspace(_unitOfWork, session.SessionContext, id, cancellationToken);
		await LoadAsync(cancellationToken);
	}

	public Task DeleteAllWorkspacesAsync(CancellationToken cancellationToken = default) => TrackAsync(DeleteAllWorkspacesCoreAsync(cancellationToken));

	private async Task DeleteAllWorkspacesCoreAsync(CancellationToken cancellationToken)
	{
		await APIService.DeleteAllWorkspaces(_unitOfWork, session.SessionContext, cancellationToken);
		await LoadAsync(cancellationToken);
	}

	private async Task TrackAsync(Task task)
	{
		LoadingQueue.Append(task);
		await task;
	}

	private void HandleLoadingQueueChanged(object? sender, EventArgs args) => session.Redraw();
}