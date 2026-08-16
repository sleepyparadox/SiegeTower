using System.Net.Http.Json;
using SiegeTower.Data.Graph.File;

namespace SiegeTower.Client.Services.Workspace;

public sealed class WorkspaceFileService
{
	private readonly Session session;

	public WorkspaceFileService(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
	}

	public async Task<IReadOnlyList<FileRow>> GetFiles(bool includeContents, CancellationToken cancellationToken = default)
	{
		var workspaceId = session.SessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request workspace files.");
		}

		var route = $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/file?contents={includeContents.ToString().ToLowerInvariant()}";
		return await session.SessionServices.HttpClient.GetFromJsonAsync<List<FileRow>>(route, cancellationToken)
			?? [];
	}
}
