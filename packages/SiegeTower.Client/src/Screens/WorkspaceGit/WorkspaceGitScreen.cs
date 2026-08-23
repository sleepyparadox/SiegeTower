using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceGit;

public sealed class WorkspaceGitScreen : Screen
{
	private readonly GraphCache _unitOfWork = new();
	readonly Session session;

	public WorkspaceGitScreen(Session session)
		: base("Workspace Git")
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
				new("Git", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceGitScreen(session.SessionContext.WorkspaceID))),
				new("Files", () => session.NavigateTo(session.GetNavigationUrlToWorkspaceFilesScreen(session.SessionContext.WorkspaceID)))
			]
		};
		ToolbarGrid = new() { Toolbars = [WorkspaceToolbar] };
		WorkspaceGitAuthContent = new();
		WorkspaceProjectListContent = new(this);
		WorkspaceProjectAddContent = new(this);
		DockGrid = new DockGrid([WorkspaceProjectListContent], [WorkspaceGitAuthContent], [WorkspaceProjectAddContent]);
	}

	public DockGrid DockGrid { get; }

	public Toolbar WorkspaceToolbar { get; }

	public ToolbarGrid ToolbarGrid { get; }

	public WorkspaceGitAuthContent WorkspaceGitAuthContent { get; }

	public WorkspaceProjectListContent WorkspaceProjectListContent { get; }

	public WorkspaceProjectAddContent WorkspaceProjectAddContent { get; }

	public GitStatus Status { get; private set; } = new();

	public override Task Load() => LoadAsync();

	public Task LoadAsync(CancellationToken cancellationToken = default) => TrackAsync(LoadCoreAsync(cancellationToken));

	private async Task LoadCoreAsync(CancellationToken cancellationToken)
	{
		Status = await WorkspaceGitService.GetGitStatusAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, cancellationToken);
		WorkspaceProjectListContent.Projects = await WorkspaceProjectService.GetProjectsAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, cancellationToken);
		session.Redraw();
	}

	public Task AddProjectAsync(CancellationToken cancellationToken = default) => TrackAsync(AddProjectCoreAsync(cancellationToken));

	private async Task AddProjectCoreAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(WorkspaceProjectAddContent.Namespace)
			|| string.IsNullOrWhiteSpace(WorkspaceProjectAddContent.GitRepo)
			|| WorkspaceProjectAddContent.IsAdding)
		{
			return;
		}

		WorkspaceProjectAddContent.IsAdding = true;
		WorkspaceProjectAddContent.Error = null;
		try
		{
			await WorkspaceProjectService.AddProjectAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient,
				new(WorkspaceProjectAddContent.Namespace.Trim(), WorkspaceProjectAddContent.GitRepo.Trim()), cancellationToken);
			WorkspaceProjectAddContent.Namespace = string.Empty;
			WorkspaceProjectAddContent.GitRepo = string.Empty;
			await LoadAsync(cancellationToken);
		}
		catch (Exception exception)
		{
			WorkspaceProjectAddContent.Error = exception;
		}
		finally
		{
			WorkspaceProjectAddContent.IsAdding = false;
			session.Redraw();
		}
	}

	public Task PullProjectAsync(string @namespace, CancellationToken cancellationToken = default) => TrackAsync(PullProjectCoreAsync(@namespace, cancellationToken));

	private async Task PullProjectCoreAsync(string @namespace, CancellationToken cancellationToken)
	{
		await WorkspaceProjectService.PullProjectAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, @namespace, cancellationToken);
		await LoadAsync(cancellationToken);
	}

	public Task GenerateAccessTokenAsync(CancellationToken cancellationToken = default) => TrackAsync(GenerateAccessTokenCoreAsync(cancellationToken));

	private async Task GenerateAccessTokenCoreAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(WorkspaceGitAuthContent.PrivateKey) || WorkspaceGitAuthContent.IsGenerating)
		{
			return;
		}

		WorkspaceGitAuthContent.IsGenerating = true;
		WorkspaceGitAuthContent.Error = null;
		session.Redraw();
		try
		{
			Status = await WorkspaceGitService.GenerateGithubAccessTokenAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, new GithubAccessTokenRequest
			{
				AppId = WorkspaceGitAuthContent.AppId,
				InstallationId = WorkspaceGitAuthContent.InstallationId,
				PrivateKey = WorkspaceGitAuthContent.PrivateKey
			}, cancellationToken);
			WorkspaceGitAuthContent.PrivateKey = string.Empty;
		}
		catch (Exception exception)
		{
			WorkspaceGitAuthContent.Error = exception;
		}
		finally
		{
			WorkspaceGitAuthContent.IsGenerating = false;
			session.Redraw();
		}
	}

	private async Task TrackAsync(Task task)
	{
		LoadingQueue.Append(task);
		await task;
	}

	private void HandleLoadingQueueChanged(object? sender, EventArgs args) => session.Redraw();

	public string GetTimeRemainingText()
	{
		if (!Status.GithubAccessTokenExpiresAtUtc.HasValue)
		{
			return "none";
		}

		var remaining = Status.GithubAccessTokenExpiresAtUtc.Value - DateTime.UtcNow;
		if (remaining <= TimeSpan.Zero)
		{
			return "expired";
		}

		return $"{(int)remaining.TotalHours}h {remaining.Minutes}m {remaining.Seconds}s";
	}
}