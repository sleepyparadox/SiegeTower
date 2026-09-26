public sealed class TreeControl : Component, IControlComponent, IParentOf<TreeNode>
{
	public ComponentRefList<TreeNode> Children { get; set; } = new();

	public TreeControl(Entity entity) : base(entity)
	{
	}
}