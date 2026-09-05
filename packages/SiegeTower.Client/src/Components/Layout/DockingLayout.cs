public sealed class DockingLayout : ScreenLayoutChild, IParentOf<DockNode>
{
	public ComponentRefList<DockNode> Children { get; set; } = new();

	public DockingLayout(Entity entity) : base(entity)
	{
	}
}