public sealed class DockWindowDragOperation : Component
{
	public ComponentRef<DockWindow> Window { get; set; }
	public ComponentRef<DockWindowGroup> SourceGroup { get; set; }
	public ComponentRef<DockWindowGroup> TargetGroup { get; set; }
	public DockWindowDropPosition? TargetPosition { get; set; }

	public DockWindowDragOperation(Entity entity, DockWindow window, DockWindowGroup sourceGroup) : base(entity)
	{
		Window = window;
		SourceGroup = sourceGroup;
		TargetGroup = new ComponentRef<DockWindowGroup>();
	}
}

public enum DockWindowDropPosition
{
	Left,
	Top,
	Right,
	Bottom,
	Center
}