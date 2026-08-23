using System.Net.Http.Json;
using SiegeTower.Data.Graph.File;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Services.Workspace;

public static class WorkspaceFileService
{
	public static async Task<IReadOnlyList<FileRow>> GetFiles(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, bool includeContents, CancellationToken cancellationToken = default)
	{
		var workspaceId = sessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request workspace files.");
		}

		var route = $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/file?contents={includeContents.ToString().ToLowerInvariant()}";
		return await httpClient.GetFromJsonAsync<List<FileRow>>(route, cancellationToken)
			?? [];
	}
}
