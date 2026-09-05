public sealed class DockWindowControlLayout : Component, IRequires<DockWindow>, IRequires<ControlLayout>
{
	public DockWindowControlLayout(Entity entity) : base(entity)
	{
	}
}