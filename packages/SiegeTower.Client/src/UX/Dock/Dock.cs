namespace SiegeTower.Client.UX;

public sealed class Dock
{
	public List<IDockContent> Contents { get; } = [];

	public IDockContent? ActiveContent { get; set; }

	public bool IsHidden { get; set; }

	public bool IsHiddenOnLastDetach { get; set; }
}