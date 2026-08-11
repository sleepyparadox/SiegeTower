using SiegeTower.Client.Screens;

namespace SiegeTower.Client;

public sealed class AppService
{
	public AppService()
	{
		ActiveScreen = new PodListScreen(this);
	}

	public Screen ActiveScreen { get; private set; }

	public SessionContext SessionContext { get; } = new() { ApiBase = "localhost:5006/api" };

	public event EventHandler? RedrawRequested;

	public void Redraw()
	{
		RedrawRequested?.Invoke(this, EventArgs.Empty);
	}

	public void SetActiveScreen(Screen screen)
	{
		ArgumentNullException.ThrowIfNull(screen);
		ActiveScreen = screen;
		Redraw();
	}
}
