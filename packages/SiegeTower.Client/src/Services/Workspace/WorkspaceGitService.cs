using System.Net.Http.Json;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Services.Workspace;

public static class WorkspaceGitService
{
	public static async Task<GitStatus> GetGitStatusAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		var route = GetRoute(sessionContext);
		return await httpClient.GetFromJsonAsync<GitStatus>(route, cancellationToken)
			?? new GitStatus();
	}

	public static async Task<GitStatus> GenerateGithubAccessTokenAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, GithubAccessTokenRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		using var response = await httpClient.PostAsJsonAsync(
			$"{GetRoute(sessionContext)}/github-access-token",
			request,
			cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<GitStatus>(cancellationToken: cancellationToken)
			?? new GitStatus();
	}

	static string GetRoute(SessionContext sessionContext)
	{
		var workspaceId = sessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request Git status.");
		}

		return $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/git";
	}
}