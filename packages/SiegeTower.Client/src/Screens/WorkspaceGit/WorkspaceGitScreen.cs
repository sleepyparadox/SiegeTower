using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data;

namespace SiegeTower.Client.Screens.WorkspaceGit;

public sealed class WorkspaceGitScreen : Screen
{
	readonly Session session;

	public WorkspaceGitScreen(Session session)
		: base("Workspace Git")
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

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		Status = await session.SessionServices.WorkspaceGitService.GetGitStatusAsync(cancellationToken);
		WorkspaceProjectListContent.Projects = await session.SessionServices.WorkspaceProjectService.GetProjectsAsync(cancellationToken);
		session.Redraw();
	}

	public async Task AddProjectAsync(CancellationToken cancellationToken = default)
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
			await session.SessionServices.WorkspaceProjectService.AddProjectAsync(
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

	public async Task PullProjectAsync(string @namespace, CancellationToken cancellationToken = default)
	{
		await session.SessionServices.WorkspaceProjectService.PullProjectAsync(@namespace, cancellationToken);
		await LoadAsync(cancellationToken);
	}

	public async Task GenerateAccessTokenAsync(CancellationToken cancellationToken = default)
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
			Status = await session.SessionServices.WorkspaceGitService.GenerateGithubAccessTokenAsync(new GithubAccessTokenRequest
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