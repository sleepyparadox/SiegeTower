using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens;

public sealed class ColorDockContent : IDockContent
{
	#region  IDockContent

	string IDockContent.Name { get => Name; }

	Dock? IDockContent.Parent { get; set; }

	#endregion
	
	public string Name { get; init; } = string.Empty;

	public string Color { get; init; } = "transparent";
}
