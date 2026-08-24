using System.Net.Http.Json;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Services.API;

public static class APIService
{
	public static async Task<IReadOnlyList<T>> Get<T>(GraphCache cache, SessionContext sessionContext, CancellationToken cancellationToken = default)
		where T : class
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		var route = typeof(T) switch
		{
			var resourceType when resourceType == typeof(WorkspaceRow) => "workspace",
			_ => throw new ArgumentException($"Unsupported API resource type '{typeof(T).Name}'.", nameof(T))
		};

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, route);
		return await httpClient.GetFromJsonAsync<List<T>>(requestUri, cancellationToken)
			?? [];
	}

	public static async Task<WorkspaceRow> CreateWorkspace(GraphCache cache, SessionContext sessionContext, string name, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, "workspace");
		using var response = await httpClient.PostAsJsonAsync(requestUri, new { Name = name }, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<WorkspaceRow>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("The API returned an empty workspace response.");
	}

	public static async Task DeleteWorkspace(GraphCache cache, SessionContext sessionContext, string name, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, $"workspace/{System.Uri.EscapeDataString(name)}");
		using var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	public static async Task DeleteAllWorkspaces(GraphCache cache, SessionContext sessionContext, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, "workspace-all");
		using var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	public static async Task<GithubAccessToken> GenerateGithubAccessToken(GraphCache cache, SessionContext sessionContext, GithubAccessTokenRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sessionContext);
		ArgumentNullException.ThrowIfNull(request);

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, "github-access-token");
		using var response = await httpClient.PostAsJsonAsync(requestUri, request, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<GithubAccessToken>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("The API returned an empty GitHub access token response.");
	}

	private static System.Uri BuildRequestUri(string apiBase, string route)
	{
		if (string.IsNullOrWhiteSpace(apiBase))
		{
			throw new ArgumentException("API base URL is required.", nameof(apiBase));
		}

		var normalizedBase = apiBase.Contains("://", StringComparison.Ordinal)
			? apiBase
			: $"http://{apiBase}";

		return new System.Uri($"{normalizedBase.TrimEnd('/')}/{route}", System.UriKind.Absolute);
	}
}
