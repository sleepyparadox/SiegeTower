namespace SiegeTower.Client.Screens;

public sealed class Dock
{
	public IReadOnlyList<object> Contents { get; init; } = [];

	public object? ActiveContent { get; set; }

	public Type? ActiveContentType => ActiveContent?.GetType();
}