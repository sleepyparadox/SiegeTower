namespace SiegeTower.Client;

public sealed class ToolbarDragStarted : SessionEvent
{
	public Toolbar Toolbar { get; }

	public ToolbarDragStarted(Toolbar toolbar, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		Toolbar = toolbar;
	}
}

public sealed class ToolbarDragTargetChanged : SessionEvent
{
	public Toolbar TargetToolbar { get; }
	public ToolbarDropPosition Position { get; }

	public ToolbarDragTargetChanged(Toolbar targetToolbar, ToolbarDropPosition position, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		TargetToolbar = targetToolbar;
		Position = position;
	}
}

public sealed class ToolbarDragStopped : SessionEvent
{
	public Toolbar TargetToolbar { get; }
	public ToolbarDropPosition Position { get; }

	public ToolbarDragStopped(Toolbar targetToolbar, ToolbarDropPosition position, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		TargetToolbar = targetToolbar;
		Position = position;
	}
}

public sealed class ToolbarDragCanceled : SessionEvent
{
	public ToolbarDragCanceled(EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
	}
}