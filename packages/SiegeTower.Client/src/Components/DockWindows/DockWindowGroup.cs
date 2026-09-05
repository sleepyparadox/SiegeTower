public sealed class DockWindowGroup : DockNode, IParentOf<DockWindow>
{
	public ComponentRefList<DockWindow> Children { get; set; } = new();
	public ComponentRef<DockWindow> ActiveWindow { get; set; } = new();

	public DockWindowGroup(Entity entity) : base(entity)
	{
	}
}