public abstract class DockNode : Component, IChildOf<DockingLayout>, IChildOf<DockContainer>
{
	ComponentRef<DockingLayout> IChildOf<DockingLayout>.Parent { get; set; } = new();
	ComponentRef<DockContainer> IChildOf<DockContainer>.Parent { get; set; } = new();

	protected DockNode(Entity entity) : base(entity)
	{
	}
}

public enum DockOrientation
{
	Horizontal,
	Vertical
}

public sealed class DockContainer : DockNode, IParentOf<DockNode>
{
	public ComponentRefList<DockNode> Children { get; set; } = new();
	public DockOrientation Orientation { get; }
	public bool IsFixedWidth { get; set; }
	public int? WidthInGridUnits { get; set; }

	public DockContainer(Entity entity, DockOrientation orientation) : base(entity)
	{
		Orientation = orientation;
	}
}