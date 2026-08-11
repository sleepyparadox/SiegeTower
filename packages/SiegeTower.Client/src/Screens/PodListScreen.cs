using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens;

public sealed class PodListScreen : Screen
{
	private readonly AppService appService;

	public PodListScreen(AppService appService)
		: base("Pods")
	{
		ArgumentNullException.ThrowIfNull(appService);
		this.appService = appService;
		PodListDockContent = new();
		DockGrid = new DockGrid(
			[
				PodListDockContent,
				new ColorDockContent { Color = "Red" },
				new ColorDockContent { Color = "Blue" }
			],
			[
				new ColorDockContent { Color = "Yellow" },
				new ColorDockContent { Color = "Green" }
			],
			[
				new ColorDockContent { Color = "Purple" },
				new ColorDockContent { Color = "Orange" }
			]);
		appService.SetActiveScreen(this);
	}

	public IReadOnlyList<Pod> Pods { get; private set; } = [];

	public PodListDockContent PodListDockContent { get; }

	public DockGrid DockGrid { get; }

	public Task LoadAsync(CancellationToken cancellationToken = default)
	{
		var fakeData = new[]
		{
			new Pod("api", "default"),
			new Pod("frontend", "default"),
			new Pod("worker", "jobs")
		};
		Pods = fakeData;
		PodListDockContent.Pods = fakeData;
		appService.Redraw();
		return Task.CompletedTask;
	}
}