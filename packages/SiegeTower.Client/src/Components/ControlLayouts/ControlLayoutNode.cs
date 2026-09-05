public enum ControlLayoutOrientation
{
	Row,
	Stack
}

public sealed class ControlLayoutNode : Component,
	IChildOf<ControlLayout>,
	IChildOf<ControlLayoutNode>,
	IParentOf<ControlLayoutNode>,
	IParentOf<ControlLayoutControl>
{
	ComponentRef<ControlLayout> IChildOf<ControlLayout>.Parent { get; set; } = new();
	ComponentRef<ControlLayoutNode> IChildOf<ControlLayoutNode>.Parent { get; set; } = new();
	ComponentRefList<ControlLayoutNode> IParentOf<ControlLayoutNode>.Children { get; set; } = new();
	ComponentRefList<ControlLayoutControl> IParentOf<ControlLayoutControl>.Children { get; set; } = new();

	public ControlLayoutOrientation Orientation { get; }

	public ControlLayoutNode(Entity entity, ControlLayoutOrientation orientation) : base(entity)
	{
		Orientation = orientation;
	}
}