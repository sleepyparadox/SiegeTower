using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SiegeTower.Client.Services.Ollama;

public sealed class OllamaService
{
	private const string OllamaBasePath = "/ollama/api";
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	private readonly Session session;

	public OllamaService(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
	}

	public async Task<IReadOnlyList<OllamaModel>> ListModelsAsync(CancellationToken cancellationToken = default)
	{
		var response = await session.SessionServices.HttpClient.GetFromJsonAsync<OllamaTagsResponse>($"{OllamaBasePath}/tags", JsonOptions, cancellationToken);
		return response?.Models ?? [];
	}

	public async Task PullModelAsync(string model, Action<string>? onStatus = null, CancellationToken cancellationToken = default)
	{
		using var response = await SendStreamingPostAsync($"{OllamaBasePath}/pull", new { Name = model, Stream = true }, cancellationToken);
		response.EnsureSuccessStatusCode();
		await ReadNdjsonAsync(response, item => onStatus?.Invoke(item.Status ?? "Downloading..."), cancellationToken);
	}

	public async Task DeleteModelAsync(string model, CancellationToken cancellationToken = default)
	{
		using var response = await session.SessionServices.HttpClient.SendAsync(
			new HttpRequestMessage(HttpMethod.Delete, $"{OllamaBasePath}/delete")
			{
				Content = JsonContent.Create(new { Name = model }, options: JsonOptions)
			},
			cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	public async Task ChatAsync(
		string model,
		IReadOnlyList<OllamaChatMessage> messages,
		Action<string> onToken,
		CancellationToken cancellationToken = default)
	{
		using var response = await SendStreamingPostAsync(
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

	private async Task<HttpResponseMessage> SendStreamingPostAsync(string route, object body, CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, route)
		{
			Content = JsonContent.Create(body, options: JsonOptions)
		};
		return await session.SessionServices.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
	}
}

public sealed class OllamaTagsResponse
{
	public List<OllamaModel> Models { get; set; } = [];
}

public sealed class OllamaModel
{
	public string Name { get; set; } = string.Empty;
	public long Size { get; set; }
	public string Digest { get; set; } = string.Empty;
}

public sealed record OllamaChatMessage(string Role, string Content);

internal sealed class OllamaStreamResponse
{
	public string? Status { get; set; }
	public OllamaChatMessage? Message { get; set; }
	[JsonPropertyName("done")]
	public bool Done { get; set; }
}
