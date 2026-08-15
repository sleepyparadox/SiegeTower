using SiegeTower.Client.Services.Ollama;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class Chat
{
	public string Summary { get; set; } = string.Empty;

	public List<OllamaChatMessage> History { get; } = [];

	public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
}