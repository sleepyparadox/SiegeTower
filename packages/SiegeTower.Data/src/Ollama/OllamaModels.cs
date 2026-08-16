using System.Text.Json.Serialization;

namespace SiegeTower.Data.Ollama;

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

public sealed class OllamaStreamResponse
{
	public string? Status { get; set; }
	public OllamaChatMessage? Message { get; set; }
	[JsonPropertyName("done")]
	public bool Done { get; set; }
}