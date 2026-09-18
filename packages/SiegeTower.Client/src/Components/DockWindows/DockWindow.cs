public sealed class DockWindow : Component, IChildOf<DockWindowGroup>
{
	public string Name { get; set; }
	public string Contents { get; set; }
	public ComponentRef<DockWindowGroup> Parent { get; set; } = new();

	public DockWindow(Entity entity, string name, string contents) : base(entity)
	{
		Name = name;
		Contents = contents;
	}
}