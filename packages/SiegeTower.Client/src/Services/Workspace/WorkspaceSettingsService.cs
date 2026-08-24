using System.Net.Http.Json;
using SiegeTower.Data;

namespace SiegeTower.Client.Services.Workspace;

public static class WorkspaceSettingsService
{
	public static async Task<WorkspaceSettings> GetAsync(SessionContext sessionContext, HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetFromJsonAsync<WorkspaceSettings>(GetRoute(sessionContext), cancellationToken)
			?? new WorkspaceSettings();
	}

	public static async Task<WorkspaceSettings> SaveAsync(SessionContext sessionContext, HttpClient httpClient, WorkspaceSettings settings, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(settings);
		using var response = await httpClient.PostAsJsonAsync(GetRoute(sessionContext), settings, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<WorkspaceSettings>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("The workspace returned an empty settings response.");
	}

	private static string GetRoute(SessionContext sessionContext)
	{
		var workspaceId = sessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request workspace settings.");
		}

		return $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/workspace/settings";
	}
}
