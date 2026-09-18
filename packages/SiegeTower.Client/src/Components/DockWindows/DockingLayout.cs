public sealed class DockingLayout : ScreenLayoutChild, IParentOf<DockLayoutNode>
{
	public ComponentRefList<DockLayoutNode> Children { get; set; } = new();

	public DockingLayout(Entity entity) : base(entity)
	{
	}
}