using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class ChatPrimaryContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Chat";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public OllamaScreen OllamaScreen { get; set; } = null!;
}