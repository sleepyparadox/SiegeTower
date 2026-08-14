using SiegeTower.Client.Screens;
using SiegeTower.Client.UX;

namespace SiegeTower.Client;

public sealed class AppService
{
	public AppService()
	{
		ActiveScreen = new PodListScreen(this);
	}

	public Screen ActiveScreen { get; private set; }

	public SessionContext SessionContext { get; } = new() { ApiBase = "localhost:5006/api" };

	public DragOperation DragOperation { get; } = new();

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

	public void DragStart(object target)
	{
		ArgumentNullException.ThrowIfNull(target);
		DragOperation.Target = target;
		Console.WriteLine($"Drag started: {target.GetType().Name}");
		Redraw();
	}

	public void DragStop(object target)
	{
		ArgumentNullException.ThrowIfNull(target);
		Console.WriteLine($"Drag stopped: {DragOperation.Target?.GetType().Name ?? "none"} over {target.GetType().Name}");
		DragOperation.Target = null;
		Redraw();
	}

	public void DragStop()
	{
		if (DragOperation.Target is null)
		{
			return;
		}

		Console.WriteLine($"Drag stopped: {DragOperation.Target.GetType().Name} over no supported drop target");
		DragOperation.Target = null;
		Redraw();
	}
}
