public sealed class ToolbarLayout : ScreenLayoutChild, IParentOf<Toolbar>
{
	public ComponentRefList<Toolbar> Children { get; set; } = new();

	public ToolbarLayout(Entity entity) : base(entity)
	{
	}
}