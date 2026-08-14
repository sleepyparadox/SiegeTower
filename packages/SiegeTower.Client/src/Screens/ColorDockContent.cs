using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens;

public sealed class ColorDockContent : IDockContent
{
	public string Name { get; init; } = string.Empty;

	public string Color { get; init; } = "transparent";
}
