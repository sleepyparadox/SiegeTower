namespace SiegeTower.Client;

public sealed class TreeNodeToggleEvent : ClickedEvent
{
	public TreeNode Node { get; }

	public TreeNodeToggleEvent(TreeNode node, EventArgs? userInteractionEvent = null)
		: base(node, userInteractionEvent)
	{
		Node = node;
	}
}