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

	public async Task<OllamaChatResponse> ChatAsync(
		IReadOnlyList<OllamaChatMessage> messages,
		IReadOnlyList<OllamaToolDefinition> tools,
		TimeSpan timeout,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(messages);
		ArgumentNullException.ThrowIfNull(tools);
		if (timeout <= TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException(nameof(timeout));
		}

		using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutSource.CancelAfter(timeout);
		using var response = await httpClient.PostAsJsonAsync("api/chat", new { Model = "qwen3.5:2b", Messages = messages, Tools = tools, Stream = false }, JsonOptions, timeoutSource.Token);
		response.EnsureSuccessStatusCode();
		var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, timeoutSource.Token)
			?? throw new InvalidOperationException("Ollama returned an invalid response.");
		return result;
	}
}
