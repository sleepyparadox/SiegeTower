using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Ollama;

public sealed class ModelsContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Models";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public OllamaScreenData Data { get; }

	public ModelsContent(OllamaScreenData data) => Data = data ?? throw new ArgumentNullException(nameof(data));
}