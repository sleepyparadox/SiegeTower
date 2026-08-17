using System.Net.Http.Json;
using SiegeTower.Data;

namespace SiegeTower.Client.Services.Workspace;

public sealed class WorkspaceGitService
{
	readonly Session session;

	public WorkspaceGitService(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
	}

	public async Task<GitStatus> GetGitStatusAsync(CancellationToken cancellationToken = default)
	{
		var route = GetRoute();
		return await session.SessionServices.HttpClient.GetFromJsonAsync<GitStatus>(route, cancellationToken)
			?? new GitStatus();
	}

	public async Task<GitStatus> GenerateGithubAccessTokenAsync(GithubAccessTokenRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		using var response = await session.SessionServices.HttpClient.PostAsJsonAsync(
			$"{GetRoute()}/github-access-token",
			request,
			cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<GitStatus>(cancellationToken: cancellationToken)
			?? new GitStatus();
	}

	string GetRoute()
	{
		var workspaceId = session.SessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request Git status.");
		}

		return $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/git";
	}
}