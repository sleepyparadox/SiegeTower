using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Ollama;
using SiegeTower.Client.UX;
using SiegeTower.Data.Ollama;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class OllamaScreen : Screen
{
	private readonly GraphCache _unitOfWork = new();
	private readonly Session session;

	public OllamaScreen(Session session)
		: base("Ollama")
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
		LoadingQueue.Changed += HandleLoadingQueueChanged;
		ChatHistoryContent = new() { OllamaScreen = this };
		ChatPrimaryContent = new() { OllamaScreen = this };
		ModelsContent = new() { OllamaScreen = this };
		DockGrid = new DockGrid([ChatHistoryContent], [ChatPrimaryContent], [ModelsContent]);
		NewChat();
	}

	public DockGrid DockGrid { get; }

	public SessionServices SessionServices => session.SessionServices;

	internal GraphCache UnitOfWork => _unitOfWork;

	internal SessionContext SessionContext => session.SessionContext;

	public ChatHistoryContent ChatHistoryContent { get; }

	public ChatPrimaryContent ChatPrimaryContent { get; }

	public ModelsContent ModelsContent { get; }

	public IReadOnlyList<Chat> Chats { get; private set; } = [];

	public Chat CurrentChat { get; private set; } = null!;

	public IReadOnlyList<OllamaModel> Models { get; private set; } = [];

	public override Task Load() => LoadModelsAsync();

	public void Redraw()
	{
		session.Redraw();
	}

	public void NewChat()
	{
		var chat = new Chat { Summary = "New chat" };
		Chats = [chat, .. Chats];
		CurrentChat = chat;
		session.Redraw();
	}

	public void OpenChat(Chat chat)
	{
		ArgumentNullException.ThrowIfNull(chat);
		CurrentChat = chat;
		session.Redraw();
	}

	public Task LoadModelsAsync(CancellationToken cancellationToken = default) => TrackAsync(LoadModelsCoreAsync(cancellationToken));

	private async Task LoadModelsCoreAsync(CancellationToken cancellationToken)
	{
		Models = await OllamaService.ListModelsAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, cancellationToken);
		session.Redraw();
	}

	public Task DeleteModelAsync(string model, CancellationToken cancellationToken = default) => TrackAsync(DeleteModelCoreAsync(model, cancellationToken));

	private async Task DeleteModelCoreAsync(string model, CancellationToken cancellationToken)
	{
		await OllamaService.DeleteModelAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, model, cancellationToken);
		Models = Models.Where(item => !string.Equals(item.Name, model, StringComparison.OrdinalIgnoreCase)).ToArray();
		session.Redraw();
	}

	public Task AddModelAsync(string model, CancellationToken cancellationToken = default) => TrackAsync(AddModelCoreAsync(model, cancellationToken));

	private async Task AddModelCoreAsync(string model, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(model);
		await OllamaService.PullModelAsync(_unitOfWork, session.SessionContext, session.SessionServices.HttpClient, model, cancellationToken: cancellationToken);
		await LoadModelsCoreAsync(cancellationToken);
	}

	private async Task TrackAsync(Task task)
	{
		LoadingQueue.Append(task);
		await task;
	}

	private void HandleLoadingQueueChanged(object? sender, EventArgs args) => session.Redraw();
}