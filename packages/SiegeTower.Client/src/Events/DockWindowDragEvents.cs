namespace SiegeTower.Client;

public sealed class DockWindowActivated : SessionEvent
{
	public DockWindow Window { get; }

	public DockWindowActivated(DockWindow window, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		Window = window;
	}
}

public sealed class DockWindowDragStarted : SessionEvent
{
	public DockWindow Window { get; }

	public DockWindowDragStarted(DockWindow window, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		Window = window;
	}
}

public sealed class DockWindowDragTargetChanged : SessionEvent
{
	public DockWindowGroup TargetGroup { get; }
	public DockWindowDropPosition Position { get; }

	public DockWindowDragTargetChanged(DockWindowGroup targetGroup, DockWindowDropPosition position, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		TargetGroup = targetGroup;
		Position = position;
	}
}

public sealed class DockWindowDragStopped : SessionEvent
{
	public DockWindowGroup TargetGroup { get; }
	public DockWindowDropPosition Position { get; }

	public DockWindowDragStopped(DockWindowGroup targetGroup, DockWindowDropPosition position, EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
		TargetGroup = targetGroup;
		Position = position;
	}
}

public sealed class DockWindowDragCanceled : SessionEvent
{
	public DockWindowDragCanceled(EventArgs? userInteractionEvent = null) : base(userInteractionEvent)
	{
	}
}