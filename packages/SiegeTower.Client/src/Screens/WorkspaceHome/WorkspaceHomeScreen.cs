using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class WorkspaceHomeScreen : Screen
{
	private readonly GraphCache _unitOfWork = new();
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
				new("Files", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceFilesScreen(session.SessionContext.WorkspaceID)))
			]
		};
		ToolbarGrid = new() { Toolbars = [WorkspaceToolbar] };
		OperationHistoryContent = new(this);
		GitCloneOperationDockContent = new();
		GitCreateBranchOperationDockContent = new();
		GitPushOperationDockContent = new();
		GitCommitOperationDockContent = new();
		PromptOperationDockContent = new();
		WorkspaceSettingsDockContent = new(this);
		DockGrid = new(
			[],
			[OperationHistoryContent],
			[GitCloneOperationDockContent, GitCreateBranchOperationDockContent, GitPushOperationDockContent, GitCommitOperationDockContent, PromptOperationDockContent, WorkspaceSettingsDockContent]);
	}

	public Toolbar WorkspaceToolbar { get; }

	public ToolbarGrid ToolbarGrid { get; }

	public OperationHistoryContent OperationHistoryContent { get; }

	public DockGrid DockGrid { get; }

	public GitCloneOperationDockContent GitCloneOperationDockContent { get; }

	public GitCreateBranchOperationDockContent GitCreateBranchOperationDockContent { get; }

	public GitPushOperationDockContent GitPushOperationDockContent { get; }

	public GitCommitOperationDockContent GitCommitOperationDockContent { get; }

	public PromptOperationDockContent PromptOperationDockContent { get; }

	public WorkspaceSettingsDockContent WorkspaceSettingsDockContent { get; }

	public SessionContext SessionContext => session.SessionContext;

	public SessionServices SessionServices => session.SessionServices;

	public void Redraw() => session.Redraw();

	internal GraphCache UnitOfWork => _unitOfWork;

	public override Task Load() => LoadAsync();

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		var task = LoadCoreAsync(cancellationToken);
		LoadingQueue.Append(task);
		await task;
	}

	public async Task SendMethod(Operation operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		await WorkspaceOperationService.SendAsync(
			new WorkspaceOperation { ID = Guid.NewGuid(), Operation = operation },
			session.SessionContext,
			session.SessionServices.HttpClient);
		await LoadAsync();
	}

	private async Task LoadCoreAsync(CancellationToken cancellationToken)
	{
		var settingsTask = WorkspaceSettingsService.GetAsync(session.SessionContext, session.SessionServices.HttpClient, cancellationToken);
		await Task.WhenAll(
			WorkspaceOperationService.GetOperationsAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, cancellationToken),
			WorkspaceOperationService.GetOperationLogsAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, cancellationToken),
			settingsTask);
		WorkspaceSettingsDockContent.SetSettings(await settingsTask);
		session.Redraw();
	}
}