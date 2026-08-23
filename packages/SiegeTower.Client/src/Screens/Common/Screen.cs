using SiegeTower.GraphQuery;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Common;

public abstract class Screen
{
	protected Screen(string title = "SiegeTower")
	{
		Title = title;
	}

	public string Title { get; }

	public LoadingQueue LoadingQueue { get; } = new();

	public abstract Task Load();
}
