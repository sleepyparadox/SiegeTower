namespace SiegeTower.Client.UX;

public sealed class Toolbar
{
	public string Name { get; init; } = string.Empty;

	public List<ToolbarItem> Items { get; init; } = [];
}

public sealed record ToolbarItem(string Label, Action OnClick);