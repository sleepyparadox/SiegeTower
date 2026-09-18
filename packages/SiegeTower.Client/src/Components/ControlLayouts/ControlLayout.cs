public sealed class ControlLayout : Component, IParentOf<ControlLayoutNode>
{
	public ComponentRefList<ControlLayoutNode> Children { get; set; } = new();

	public ControlLayout(Entity entity) : base(entity)
	{
	}
}