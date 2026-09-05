public sealed class ToolbarDragOperation : Component
{
	public ComponentRef<Toolbar> Toolbar { get; set; }
	public ComponentRef<Toolbar> TargetToolbar { get; set; } = new();
	public ToolbarDropPosition? TargetPosition { get; set; }

	public ToolbarDragOperation(Entity entity, Toolbar toolbar) : base(entity)
	{
		Toolbar = toolbar;
	}
}

public enum ToolbarDropPosition
{
	Left,
	Top,
	Right,
	Bottom
}