public class TreeNode : Component, IRequires<Element>
{
	public string Text { get; set; }
	public TreeNodeIcon Icon { get; set; }
	public bool IsExpanded { get; set; }
	public bool IsSelected { get; set; }

	public TreeNode(Entity entity, string text, TreeNodeIcon icon = TreeNodeIcon.File) : base(entity)
	{
		Text = text;
		Icon = icon;
	}
}