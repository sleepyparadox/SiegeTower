using SiegeTower.Client.Pattern;
using SiegeTower.Client.Services.Ollama;
using SiegeTower.Client.UX;
using SiegeTower.Data.Ollama;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class OllamaScreenSystem : IDataSystem
{
	public OllamaScreenSystem() { }

	public Task Load(OllamaScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		return LoadModelsAsync(data);
	}

	public Task SystemLoad(OllamaScreenData data) => Load(data);

	public void NewChat(OllamaScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		var chat = new Chat { Summary = "New chat" };
		data.Chats = [chat, .. data.Chats];
		data.CurrentChat = chat;
		data.Session.RequestRedraw();
	}

	public void OpenChat(OllamaScreenData data, Chat chat)
	{
		ArgumentNullException.ThrowIfNull(data);
		ArgumentNullException.ThrowIfNull(chat);
		data.CurrentChat = chat;
		data.Session.RequestRedraw();
	}

	public Task LoadModelsAsync(OllamaScreenData data, CancellationToken cancellationToken = default) => TrackAsync(data, LoadModelsCoreAsync(data, cancellationToken));

	private async Task LoadModelsCoreAsync(OllamaScreenData data, CancellationToken cancellationToken)
	{
		data.Models = await OllamaService.ListModelsAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, cancellationToken);
		data.Session.RequestRedraw();
	}

	public Task DeleteModelAsync(OllamaScreenData data, string model, CancellationToken cancellationToken = default) => TrackAsync(data, DeleteModelCoreAsync(data, model, cancellationToken));

	private async Task DeleteModelCoreAsync(OllamaScreenData data, string model, CancellationToken cancellationToken)
	{
		await OllamaService.DeleteModelAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, model, cancellationToken);
		data.Models = data.Models.Where(item => !string.Equals(item.Name, model, StringComparison.OrdinalIgnoreCase)).ToArray();
		data.Session.RequestRedraw();
	}

	public Task AddModelAsync(OllamaScreenData data, string model, CancellationToken cancellationToken = default) => TrackAsync(data, AddModelCoreAsync(data, model, cancellationToken));

	private async Task AddModelCoreAsync(OllamaScreenData data, string model, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(model);
		await OllamaService.PullModelAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, model, cancellationToken: cancellationToken);
		await LoadModelsCoreAsync(data, cancellationToken);
	}

	private async Task TrackAsync(OllamaScreenData data, Task task)
	{
		data.LoadingQueue.Append(task);
		await task;
	}
}
