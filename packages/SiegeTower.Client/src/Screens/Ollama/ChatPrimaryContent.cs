using SiegeTower.Client.UX;
using SiegeTower.Client.Screens.WorkspaceFiles;
using SiegeTower.Data.Ollama;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class ChatPrimaryContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Chat";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public OllamaScreenData Data { get; }

	public WorkspaceFilesScreenData? WorkspaceFilesScreen { get; set; }

	public List<OllamaChatMessage> WorkspaceHistory { get; } = [];

	public bool IsWorkspace => WorkspaceFilesScreen is not null;

	public ChatPrimaryContent(OllamaScreenData data) => Data = data ?? throw new ArgumentNullException(nameof(data));
}