namespace SiegeTower.Client.UX;

public sealed class Dock
{
	private readonly List<IDockContent> contents = [];

	public IReadOnlyList<IDockContent> Contents => contents;

	public IDockContent? ActiveContent { get; set; }

	public Type? ActiveContentType => ActiveContent?.GetType();

	internal List<IDockContent> MutableContents => contents;
}