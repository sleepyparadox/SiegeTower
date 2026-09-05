public sealed class ToolbarControl : Component, IChildOf<Toolbar>
{
	public ComponentRef<Toolbar> Parent { get; set; } = new();

	public ToolbarControl(Entity entity) : base(entity)
	{
	}
}