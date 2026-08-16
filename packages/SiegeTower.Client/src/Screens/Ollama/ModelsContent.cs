using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class ModelsContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Models";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public OllamaScreen OllamaScreen { get; set; } = null!;
}