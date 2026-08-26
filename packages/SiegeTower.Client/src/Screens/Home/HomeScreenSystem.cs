using SiegeTower.Client.Pattern;

namespace SiegeTower.Client.Screens.Home;

public sealed class HomeScreenSystem : IDataSystem
{
	public HomeScreenSystem() { }

	public Task Load(HomeScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		var task = Task.CompletedTask;
		data.LoadingQueue.Append(task);
		return task;
	}

	public Task SystemLoad(HomeScreenData data) => Load(data);
}
