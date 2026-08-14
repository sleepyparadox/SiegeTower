namespace SiegeTower.Client.UX;

public sealed class Dock
{
	public IReadOnlyList<IDockContent> Contents { get; init; } = [];

	public IDockContent? ActiveContent { get; set; }

	public Type? ActiveContentType => ActiveContent?.GetType();
}