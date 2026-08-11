namespace SiegeTower.Client.UX;

public sealed class DockGrid
{
	public DockGrid(
		IReadOnlyList<object> leftContents,
		IReadOnlyList<object> centerContents,
		IReadOnlyList<object> rightContents)
	{
		Left = CreateDock(leftContents);
		Center = CreateDock(centerContents);
		Right = CreateDock(rightContents);
	}

	public Dock Left { get; }

	public Dock Center { get; }

	public Dock Right { get; }

	private static Dock CreateDock(IReadOnlyList<object> contents)
	{
		ArgumentNullException.ThrowIfNull(contents);
		if (contents.Count == 0)
		{
			throw new ArgumentException("A dock must contain at least one content item.", nameof(contents));
		}

		return new Dock
		{
			Contents = contents,
			ActiveContent = contents[0]
		};
	}
}
