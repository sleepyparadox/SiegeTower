namespace SiegeTower.Client;

public class ClickedEvent : SessionEvent
{
	public Component Component { get; }

	public ClickedEvent(Component component, EventArgs? userInteractionEvent = null)
		: base(userInteractionEvent)
	{
		ArgumentNullException.ThrowIfNull(component);
		Component = component;
	}
}