using SiegeTower.Data.ECSPattern;

namespace SiegeTower.Client.Screens.Home;

public sealed class HomeScreenSystem : ISystem
{
	public HomeScreenSystem() { }

	public Task Load(HomeScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		var task = Task.CompletedTask;
		data.Session.LoadingQueue.Append(task);
		return task;
	}

	public async Task SystemLoad(HomeScreenData data)
	{
		await Load(data);
		data.IsLoadedOnce = true;
		data.Session.RequestRedraw();
	}
}
