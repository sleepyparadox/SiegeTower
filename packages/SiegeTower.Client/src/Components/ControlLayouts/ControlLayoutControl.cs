public sealed class ControlLayoutControl : Component, IChildOf<ControlLayoutNode>
{
	public ComponentRef<ControlLayoutNode> Parent { get; set; } = new();

	public ControlLayoutControl(Entity entity) : base(entity)
	{
	}
}