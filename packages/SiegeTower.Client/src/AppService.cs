using Microsoft.AspNetCore.Components.Web;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Screens.PodList;
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

	public void DragStop(object over)
	{
		ArgumentNullException.ThrowIfNull(over);
		if (DragOperation.Target is IDockContent dockContent)
		{
			if (over is Dock overDock)
				DockService.Attach(overDock, dockContent);
			else if (over is DockGrid.Divider dockDivider)
				DockService.Attach(dockDivider.PrimaryDock, dockContent);
			else if (over is IDockContent overDockContent)
				DockService.Attach(overDockContent.Parent!, dockContent);
		}
		else 
		{
			Console.WriteLine($"Drag stopped: {DragOperation.Target?.GetType().Name ?? "none"} over {over.GetType().Name}");
			DragOperation.Target = null;
		}
		
		Redraw();
	}

	public void DragStop(MouseEventArgs args)
	{
		if (DragOperation.Target is null)
		{
			return;
		}

		Console.WriteLine($"Drag stopped: {DragOperation.Target.GetType().Name} over no supported drop target " +
			$"at client ({args.ClientX}, {args.ClientY}), screen ({args.ScreenX}, {args.ScreenY}), " +
			$"button {args.Button}, buttons {args.Buttons}, " +
			$"modifiers [alt={args.AltKey}, ctrl={args.CtrlKey}, meta={args.MetaKey}, shift={args.ShiftKey}]");
		DragOperation.Target = null;
		Redraw();
	}
}
