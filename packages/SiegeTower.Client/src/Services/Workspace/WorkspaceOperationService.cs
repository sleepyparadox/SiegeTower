using System.Net.Http.Json;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Services.Workspace;

public static class WorkspaceOperationService
{
	public static async Task<IReadOnlyList<WorkspaceOperation>> GetOperationsAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		var operations = await httpClient.GetFromJsonAsync<List<WorkspaceOperation>>(GetRoute(sessionContext, "api/operation"), cancellationToken) ?? [];
		((GraphIndexUniqueGuid<WorkspaceOperation>)cache.GetPrimaryIndex<Guid, WorkspaceOperation>()).Store(operations);
		return operations;
	}

	public static async Task<IReadOnlyList<WorkspaceOperationLog>> GetOperationLogsAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, DateTime minCreatedAtUtc = default, CancellationToken cancellationToken = default)
	{
		var route = $"api/operation/all/log?minCreatedAtUtcTicks={minCreatedAtUtc.ToUniversalTime().Ticks}";
		var logs = await httpClient.GetFromJsonAsync<List<WorkspaceOperationLog>>(GetRoute(sessionContext, route), cancellationToken) ?? [];
		((GraphIndexUniqueOperationIDSelfID<WorkspaceOperationLog>)cache.GetPrimaryIndex<OperationLogRowKey, WorkspaceOperationLog>()).Store(logs);
		return logs;
	}

	public static async Task SendAsync(WorkspaceOperation operation, SessionContext sessionContext, HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		using var response = await httpClient.PostAsJsonAsync(GetRoute(sessionContext, "api/operation"), operation, cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	private static string GetRoute(SessionContext sessionContext, string route)
	{
		var workspaceId = sessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request operations.");
		}

		return $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/{route}";
	}
}
