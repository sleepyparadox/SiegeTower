using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;
using SiegeTower.Data.Ollama;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class OllamaScreenData : IScreenData
{
	public GraphCache Cache { get; } = new();
	public SessionData Session { get; }
	public LoadingQueue LoadingQueue { get; } = new();
	public string Title => "Ollama";
	public IReadOnlyList<Chat> Chats { get; internal set; } = [];
	public Chat CurrentChat { get; internal set; } = null!;
	public IReadOnlyList<OllamaModel> Models { get; internal set; } = [];
	public OllamaScreenSystem System { get; }
	public ChatHistoryContent ChatHistoryContent { get; }
	public ChatPrimaryContent ChatPrimaryContent { get; }
	public ModelsContent ModelsContent { get; }
	public DockGrid DockGrid { get; }

	public OllamaScreenData(SessionData session)
	{
		Session = session ?? throw new ArgumentNullException(nameof(session));
		System = new();
		ChatHistoryContent = new(this);
		ChatPrimaryContent = new(this);
		ModelsContent = new(this);
		DockGrid = new DockGrid([ChatHistoryContent], [ChatPrimaryContent], [ModelsContent]);
		System.NewChat(this);
	}
}
