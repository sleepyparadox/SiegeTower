using SiegeTower.Client.Screens.Common;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.Home;

public sealed class HomeScreen : Screen
{
	private readonly GraphCache _unitOfWork = new();

	public HomeScreen()
		: base("Home")
	{
	}

	public override Task Load()
	{
		var task = Task.CompletedTask;
		LoadingQueue.Append(task);
		return task;
	}
}
