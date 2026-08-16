using SiegeTower.Data.Ollama;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceContext
{
	public List<OllamaChatMessage> ChatHistory { get; } = [];
}