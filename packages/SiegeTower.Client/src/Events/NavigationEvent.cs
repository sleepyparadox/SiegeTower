namespace SiegeTower.Client;

public sealed class NavigationEvent : SessionEvent
{
	public Hyperlink Hyperlink { get; }

	public NavigationEvent(Hyperlink hyperlink, EventArgs? userInteractionEvent = null)
		: base(userInteractionEvent)
	{
		ArgumentNullException.ThrowIfNull(hyperlink);
		Hyperlink = hyperlink;
	}
}