using System.Net.Http.Json;
using System.Text.Json;
using SiegeTower.Data.Ollama;

namespace SiegeTower.WorkspaceHarness.Services.Ollama;

public sealed class OllamaService
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	private readonly HttpClient httpClient;

	public OllamaService(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public async Task ChatAsync(
		IReadOnlyList<OllamaChatMessage> messages,
		Action<string> onToken,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(messages);
		ArgumentNullException.ThrowIfNull(onToken);
		using var response = await httpClient.PostAsJsonAsync("api/chat", new { Model = "qwen3.5:2b", Messages = messages, Stream = true }, JsonOptions, cancellationToken);
		response.EnsureSuccessStatusCode();
		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		using var reader = new StreamReader(stream);
		while (await reader.ReadLineAsync(cancellationToken) is { } line)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				var item = JsonSerializer.Deserialize<OllamaStreamResponse>(line, JsonOptions)
					?? throw new InvalidOperationException("Ollama returned an invalid response.");
				if (!string.IsNullOrEmpty(item.Message?.Content))
				{
					onToken(item.Message.Content);
				}
			}
		}
	}
}
