public class TreeNode : Component,
	IChildOf<TreeControl>,
	IChildOf<TreeNode>,
	IParentOf<TreeNode>
{
	public string Text { get; set; }
	public TreeNodeIcon Icon { get; set; }
	public bool IsExpanded { get; set; }
	public bool IsSelected { get; set; }
	ComponentRef<TreeControl> IChildOf<TreeControl>.Parent { get; set; } = new();
	ComponentRef<TreeNode> IChildOf<TreeNode>.Parent { get; set; } = new();
	ComponentRefList<TreeNode> IParentOf<TreeNode>.Children { get; set; } = new();

	public TreeNode(Entity entity, string text, TreeNodeIcon icon = TreeNodeIcon.File) : base(entity)
	{
		Text = text;
		Icon = icon;
	}
}