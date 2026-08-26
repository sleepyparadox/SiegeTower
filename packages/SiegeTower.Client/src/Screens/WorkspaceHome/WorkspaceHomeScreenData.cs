using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class WorkspaceHomeScreenData : IScreenData
{
	public GraphCache Cache { get; } = new();
	public SessionData Session { get; }
	public bool IsLoadedOnce { get; internal set; }
	public PeriodicTimer? RefreshTimer { get; internal set; }
	public string Title => "Workspace";
	public WorkspaceHomeScreenSystem System { get; }
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

	public WorkspaceHomeScreenData(SessionData session)
	{
		Session = session ?? throw new ArgumentNullException(nameof(session));
		System = new();
		WorkspaceToolbar = new() { Name = "Workspace", Items = [new("Workspace", () => Session.NavigateTo($"workspace/{Session.Context.WorkspaceID}")), new("Files", () => Session.NavigateTo($"workspace/{Session.Context.WorkspaceID}/files"))] };
		ToolbarGrid = new() { Toolbars = [WorkspaceToolbar] };
		OperationHistoryContent = new(this);
		GitCloneOperationDockContent = new();
		GitCreateBranchOperationDockContent = new();
		GitPushOperationDockContent = new();
		GitCommitOperationDockContent = new();
		PromptOperationDockContent = new();
		WorkspaceSettingsDockContent = new(this);
		DockGrid = new([], [OperationHistoryContent], [GitCloneOperationDockContent, GitCreateBranchOperationDockContent, GitPushOperationDockContent, GitCommitOperationDockContent, PromptOperationDockContent, WorkspaceSettingsDockContent]);
	}
}
