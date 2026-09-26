namespace SiegeTower.Client;

public static class WorkspaceAPISystem
{
	public static Task<T?> Get<T>(Session session, string workspaceId, string url)
		=> HttpSystem.Get<T>(session, CreateUrl(workspaceId, url));

	public static Task<TResponse?> Post<TRequest, TResponse>(Session session, string workspaceId, string url, TRequest request)
		=> HttpSystem.Post<TRequest, TResponse>(session, CreateUrl(workspaceId, url), request);

	static string CreateUrl(string workspaceId, string url)
	{
		ArgumentException.ThrowIfNullOrEmpty(workspaceId);
		ArgumentException.ThrowIfNullOrEmpty(url);

		return $"/workspace/{Uri.EscapeDataString(workspaceId)}/api/{url.TrimStart('/')}";
	}
}