using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Services.Ollama;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class OllamaScreen : Screen
{
	private readonly Session session;

	public OllamaScreen(Session session)
		: base("Ollama")
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
		ChatHistoryContent = new() { OllamaScreen = this };
		ChatPrimaryContent = new() { OllamaScreen = this };
		ModelsContent = new() { OllamaScreen = this };
		DockGrid = new DockGrid([ChatHistoryContent], [ChatPrimaryContent], [ModelsContent]);
		NewChat();
	}

	public DockGrid DockGrid { get; }

	public SessionServices SessionServices => session.SessionServices;

	public ChatHistoryContent ChatHistoryContent { get; }

	public ChatPrimaryContent ChatPrimaryContent { get; }

	public ModelsContent ModelsContent { get; }

	public IReadOnlyList<Chat> Chats { get; private set; } = [];

	public Chat CurrentChat { get; private set; } = null!;

	public IReadOnlyList<OllamaModel> Models { get; private set; } = [];

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

	public async Task LoadModelsAsync(CancellationToken cancellationToken = default)
	{
		Models = await session.SessionServices.OllamaService.ListModelsAsync(cancellationToken);
		session.Redraw();
	}

	public async Task DeleteModelAsync(string model, CancellationToken cancellationToken = default)
	{
		await session.SessionServices.OllamaService.DeleteModelAsync(model, cancellationToken);
		Models = Models.Where(item => !string.Equals(item.Name, model, StringComparison.OrdinalIgnoreCase)).ToArray();
		session.Redraw();
	}

	public async Task AddModelAsync(string model, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(model);
		await session.SessionServices.OllamaService.PullModelAsync(model, cancellationToken: cancellationToken);
		await LoadModelsAsync(cancellationToken);
	}
}