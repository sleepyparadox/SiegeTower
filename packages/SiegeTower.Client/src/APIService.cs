using System.Net.Http.Json;
using SiegeTower.Data;

namespace SiegeTower.Client;

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
		var requestUri = BuildRequestUri(sessionContext.ApiBase, route);
		return await httpClient.GetFromJsonAsync<List<T>>(requestUri, cancellationToken)
			?? [];
	}

	private static Uri BuildRequestUri(string apiBase, string route)
	{
		if (string.IsNullOrWhiteSpace(apiBase))
		{
			throw new ArgumentException("API base URL is required.", nameof(apiBase));
		}

		var normalizedBase = apiBase.Contains("://", StringComparison.Ordinal)
			? apiBase
			: $"http://{apiBase}";

		return new Uri($"{normalizedBase.TrimEnd('/')}/{route}", UriKind.Absolute);
	}
}