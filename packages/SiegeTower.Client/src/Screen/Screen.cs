namespace SiegeTower.Client;

public class Screen : EntityStorage
{
	public Session Session { get; set; }
	public string Title { get; set; }
	
	public Screen(Session session, string title)
	{
		Session = session;
		Title = title;
	}
}