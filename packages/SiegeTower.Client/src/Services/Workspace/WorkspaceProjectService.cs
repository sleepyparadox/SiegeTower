using System.Net.Http.Json;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Services.Workspace;

public static class WorkspaceProjectService
{
	public static async Task<IReadOnlyList<WorkspaceProjectRow>> GetProjectsAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetFromJsonAsync<List<WorkspaceProjectRow>>(GetRoute(sessionContext), cancellationToken) ?? [];
	}

	public static async Task<WorkspaceProjectRow> AddProjectAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, WorkspaceProjectRow project, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(project);
		using var response = await httpClient.PostAsJsonAsync(GetRoute(sessionContext), project, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<WorkspaceProjectRow>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("The workspace returned an empty project response.");
	}

	public static async Task PullProjectAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, string @namespace, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
		using var response = await httpClient.PostAsync(
			$"{GetRoute(sessionContext)}/{System.Uri.EscapeDataString(@namespace)}/git-pull",
			null,
			cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	private static string GetRoute(SessionContext sessionContext)
	{
		var workspaceId = sessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request projects.");
		}

		return $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/project";
	}
}