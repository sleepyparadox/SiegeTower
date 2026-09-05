namespace SiegeTower.Client;

public abstract class SessionEvent
{
	public EventArgs? UserInteractionEvent { get; }

	public bool IsCanceled { get; private set; }

	protected SessionEvent(EventArgs? userInteractionEvent)
	{
		UserInteractionEvent = userInteractionEvent;
	}

	public void Cancel()
	{
		IsCanceled = true;
	}
}