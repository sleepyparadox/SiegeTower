public abstract class DockLayoutNode : Component, IChildOf<DockingLayout>, IChildOf<DockContainer>
{
	ComponentRef<DockingLayout> IChildOf<DockingLayout>.Parent { get; set; } = new();
	ComponentRef<DockContainer> IChildOf<DockContainer>.Parent { get; set; } = new();

	protected DockLayoutNode(Entity entity) : base(entity)
	{
	}
}