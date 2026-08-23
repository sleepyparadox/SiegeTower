using System.Net.Http.Json;
using System.Text.Json;
using SiegeTower.Data.Ollama;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Services.Ollama;

public interface IOllamaService
{
	Task<IReadOnlyList<OllamaModel>> ListModelsAsync(CancellationToken cancellationToken = default);
	Task PullModelAsync(string model, Action<string>? onStatus = null, CancellationToken cancellationToken = default);
	Task DeleteModelAsync(string model, CancellationToken cancellationToken = default);
	Task ChatAsync(
		string model,
		IReadOnlyList<OllamaChatMessage> messages,
		Action<string> onToken,
		CancellationToken cancellationToken = default);
	Task<IReadOnlyList<OllamaChatMessage>> ChatWorkspace(string workspaceID, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<OllamaChatMessage>> ChatWorkspace(string workspaceID, OllamaChatMessage message, CancellationToken cancellationToken = default);
}

public static class OllamaService
{
	private const string OllamaBasePath = "/ollama/api";
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	public static async Task<IReadOnlyList<OllamaModel>> ListModelsAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		var response = await httpClient.GetFromJsonAsync<OllamaTagsResponse>($"{OllamaBasePath}/tags", JsonOptions, cancellationToken);
		return response?.Models ?? [];
	}

	public static async Task PullModelAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, string model, Action<string>? onStatus = null, CancellationToken cancellationToken = default)
	{
		using var response = await SendStreamingPostAsync(httpClient, $"{OllamaBasePath}/pull", new { Name = model, Stream = true }, cancellationToken);
		response.EnsureSuccessStatusCode();
		await ReadNdjsonAsync(response, item => onStatus?.Invoke(item.Status ?? "Downloading..."), cancellationToken);
	}

	public static async Task DeleteModelAsync(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, string model, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.SendAsync(
			new HttpRequestMessage(HttpMethod.Delete, $"{OllamaBasePath}/delete")
			{
				Content = JsonContent.Create(new { Name = model }, options: JsonOptions)
			},
			cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	public static async Task ChatAsync(
		GraphCache cache,
		SessionContext sessionContext,
		HttpClient httpClient,
		string model,
		IReadOnlyList<OllamaChatMessage> messages,
		Action<string> onToken,
		CancellationToken cancellationToken = default)
	{
		using var response = await SendStreamingPostAsync(
			httpClient,
			$"{OllamaBasePath}/chat",
			new { Model = model, Messages = messages, Stream = true },
			cancellationToken);
		response.EnsureSuccessStatusCode();
		await ReadNdjsonAsync(response, item =>
		{
			if (!string.IsNullOrEmpty(item.Message?.Content))
			{
				onToken(item.Message.Content);
			}
		}, cancellationToken);
	}

	public static async Task<IReadOnlyList<OllamaChatMessage>> ChatWorkspace(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, string workspaceID, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceID);
		var route = $"/workspace/{System.Uri.EscapeDataString(workspaceID)}/api/chat";
		return await httpClient.GetFromJsonAsync<List<OllamaChatMessage>>(route, JsonOptions, cancellationToken) ?? [];
	}

	public static async Task<IReadOnlyList<OllamaChatMessage>> ChatWorkspace(GraphCache cache, SessionContext sessionContext, HttpClient httpClient, string workspaceID, OllamaChatMessage message, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceID);
		ArgumentNullException.ThrowIfNull(message);
		var route = $"/workspace/{System.Uri.EscapeDataString(workspaceID)}/api/chat";
		using var response = await httpClient.PostAsJsonAsync(route, message, JsonOptions, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<OllamaChatMessage>>(JsonOptions, cancellationToken) ?? [];
	}

	private static async Task ReadNdjsonAsync(HttpResponseMessage response, Action<OllamaStreamResponse> onItem, CancellationToken cancellationToken)
	{
		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		using var reader = new StreamReader(stream);
		while (await reader.ReadLineAsync(cancellationToken) is { } line)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				onItem(JsonSerializer.Deserialize<OllamaStreamResponse>(line, JsonOptions)
					?? throw new InvalidOperationException("Ollama returned an invalid response."));
			}
		}
	}

	private static async Task<HttpResponseMessage> SendStreamingPostAsync(HttpClient httpClient, string route, object body, CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, route)
		{
			Content = JsonContent.Create(body, options: JsonOptions)
		};
		return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
	}
}
