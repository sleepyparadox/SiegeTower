public sealed class DockContainer : DockLayoutNode, IParentOf<DockLayoutNode>
{
	public ComponentRefList<DockLayoutNode> Children { get; set; } = new();
	public DockOrientation Orientation { get; }
	public bool IsFixedWidth { get; set; }
	public int? WidthInGridUnits { get; set; }

	public DockContainer(Entity entity, DockOrientation orientation) : base(entity)
	{
		Orientation = orientation;
	}
}