using System.Net.Http.Json;
using SiegeTower.Data;

namespace SiegeTower.Client.Services.API;

public static class APIService
{
	public static async Task<IReadOnlyList<T>> Get<T>(SessionContext sessionContext, CancellationToken cancellationToken = default)
		where T : class
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		var route = typeof(T) switch
		{
			var resourceType when resourceType == typeof(Pod) => "pod",
			var resourceType when resourceType == typeof(Workspace) => "workspace",
			_ => throw new ArgumentException($"Unsupported API resource type '{typeof(T).Name}'.", nameof(T))
		};

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, route);
		return await httpClient.GetFromJsonAsync<List<T>>(requestUri, cancellationToken)
			?? [];
	}

	public static async Task<Pod> CreatePod(SessionContext sessionContext, string name, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, "pod");
		using var response = await httpClient.PostAsJsonAsync(requestUri, new { Name = name }, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<Pod>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("The API returned an empty Pod response.");
	}

	public static async Task DeleteWorkspace(SessionContext sessionContext, string name, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sessionContext);

		using var httpClient = new HttpClient();
		var requestUri = BuildRequestUri(sessionContext.ApiBaseUri, $"workspace/{System.Uri.EscapeDataString(name)}");
		using var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
		response.EnsureSuccessStatusCode();
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
