namespace SiegeTower.Client;

public static class TreeSystem
{
	public static void HandleEvent(Session session, SessionEvent sessionEvent)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(sessionEvent);

		if (sessionEvent is TreeNodeToggleEvent toggleEvent && !toggleEvent.IsCanceled)
		{
			toggleEvent.Node.IsExpanded = !toggleEvent.Node.IsExpanded;
		}
		else if (sessionEvent is ClickedEvent { Component: TreeNode clickedNode } && !sessionEvent.IsCanceled)
		{
			foreach (var node in session.ActiveScreen.SelectComponents<TreeNode>())
			{
				node.IsSelected = node == clickedNode;
			}
		}
	}
}