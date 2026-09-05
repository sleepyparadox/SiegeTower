public abstract class DockNode : Component, IChildOf<DockingLayout>, IChildOf<DockContainer>
{
	ComponentRef<DockingLayout> IChildOf<DockingLayout>.Parent { get; set; } = new();
	ComponentRef<DockContainer> IChildOf<DockContainer>.Parent { get; set; } = new();

	protected DockNode(Entity entity) : base(entity)
	{
	}
}

public abstract class DockContainer : DockNode, IParentOf<DockNode>
{
	public ComponentRefList<DockNode> Children { get; set; } = new();
	public bool IsFixedWidth { get; set; }
	public int? WidthInGridUnits { get; set; }

	protected DockContainer(Entity entity) : base(entity)
	{
	}
}