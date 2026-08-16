using SiegeTower.Client.Services.Ollama;
using SiegeTower.Data.Ollama;

namespace SiegeTower.Client.Debug;

public sealed class FakeOllamaService : IOllamaService
{
	private static readonly OllamaModel FakeModel = new()
	{
		Name = "qwen3.5:2b",
		Size = 0,
		Digest = "debug"
	};

	public Task<IReadOnlyList<OllamaModel>> ListModelsAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult<IReadOnlyList<OllamaModel>>([FakeModel]);
	}

	public Task PullModelAsync(string model, Action<string>? onStatus = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(model);
		onStatus?.Invoke($"Downloaded {model}.");
		return Task.CompletedTask;
	}

	public Task DeleteModelAsync(string model, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(model);
		return Task.CompletedTask;
	}

	public Task ChatAsync(
		string model,
		IReadOnlyList<OllamaChatMessage> messages,
		Action<string> onToken,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(model);
		ArgumentNullException.ThrowIfNull(messages);
		ArgumentNullException.ThrowIfNull(onToken);
		onToken("This is a debug response from the fake Ollama service.");
		return Task.CompletedTask;
	}

	public Task<IReadOnlyList<OllamaChatMessage>> ChatWorkspace(string workspaceID, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceID);
		return Task.FromResult<IReadOnlyList<OllamaChatMessage>>([]);
	}

	public Task<IReadOnlyList<OllamaChatMessage>> ChatWorkspace(string workspaceID, OllamaChatMessage message, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceID);
		ArgumentNullException.ThrowIfNull(message);
		return Task.FromResult<IReadOnlyList<OllamaChatMessage>>([message, new OllamaChatMessage("assistant", "This is a debug workspace response.")]);
	}
}