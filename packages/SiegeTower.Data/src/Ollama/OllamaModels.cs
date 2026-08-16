using System.Text.Json;
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

public sealed record OllamaChatMessage(
	string Role,
	string Content,
	[property: JsonPropertyName("tool_calls")] IReadOnlyList<OllamaToolCall>? ToolCalls = null);

public sealed class OllamaToolCall
{
	public string? Id { get; set; }
	public OllamaToolFunction Function { get; set; } = new();
}

public sealed class OllamaToolFunction
{
	public string Name { get; set; } = string.Empty;
	public JsonElement Arguments { get; set; }
}

public sealed class OllamaToolDefinition
{
	public string Type { get; set; } = "function";
	public OllamaToolFunctionDefinition Function { get; set; } = new();
}

public sealed class OllamaToolFunctionDefinition
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public object Parameters { get; set; } = new();
}

public sealed class OllamaChatResponse
{
	public OllamaChatMessage Message { get; set; } = new("assistant", string.Empty);

	[JsonPropertyName("done")]
	public bool Done { get; set; }

	[JsonPropertyName("done_reason")]
	public string? DoneReason { get; set; }
}

public sealed class OllamaStreamResponse
{
	public string? Status { get; set; }
	public OllamaChatMessage? Message { get; set; }
	[JsonPropertyName("done")]
	public bool Done { get; set; }
}