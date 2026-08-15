using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class ChatHistoryContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Chat History";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public OllamaScreen OllamaScreen { get; set; } = null!;
}