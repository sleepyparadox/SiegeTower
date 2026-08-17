using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Workspace;
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
		WorkspaceGitAuthContent = new();
	}

	public WorkspaceGitAuthContent WorkspaceGitAuthContent { get; }

	public GitStatus Status { get; private set; } = new();

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		Status = await session.SessionServices.WorkspaceGitService.GetGitStatusAsync(cancellationToken);
		session.Redraw();
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