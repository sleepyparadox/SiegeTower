namespace SiegeTower.Client.UX;

public sealed class DockGrid
{
	public DockGrid(
		IReadOnlyList<IDockContent> leftContents,
		IReadOnlyList<IDockContent> centerContents,
		IReadOnlyList<IDockContent> rightContents)
	{
		Left = CreateDock(leftContents);
		Center = CreateDock(centerContents);
		Right = CreateDock(rightContents);
	}

	public Dock Left { get; }

	public Dock Center { get; }

	public Dock Right { get; }

	private static Dock CreateDock(IReadOnlyList<IDockContent> contents)
	{
		ArgumentNullException.ThrowIfNull(contents);
		if (contents.Count == 0)
		{
			throw new ArgumentException("A dock must contain at least one content item.", nameof(contents));
		}

		var dock = new Dock();
		foreach (var content in contents)
		{
			DockService.Attach(dock, content);
		}

		dock.ActiveContent = contents[0];
		return dock;
	}
}
