using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.Home;

public sealed class HomeScreenData : IScreenData
{
	public GraphCache Cache { get; } = new();
	public SessionData Session { get; }
	public LoadingQueue LoadingQueue { get; } = new();
	public string Title => "Home";
	public HomeScreenSystem System { get; }

	public HomeScreenData(SessionData session)
	{
		Session = session ?? throw new ArgumentNullException(nameof(session));
		System = new();
	}
}
