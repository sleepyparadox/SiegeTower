using SiegeTower.Data;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.PodList;

public sealed class PodListScreen : Screen
{
	private readonly AppService appService;

	public PodListScreen(AppService appService)
		: base("Pods")
	{
		ArgumentNullException.ThrowIfNull(appService);
		TitleBar = new()
		{
			Title = Title,
			Breadcrumbs = ["SiegeTower", "Pods"]
		};
		FileToolbar = new() { Name = "File", Items = ["File", "Open", "Save"] };
		HelpToolbar = new() { Name = "Help", Items = ["Help"] };
		ToolbarGrid = new()
		{
			Toolbars = [FileToolbar, HelpToolbar]
		};
		this.appService = appService;
		PodListDockContent = new();
		DockGrid = new DockGrid(
			[
				PodListDockContent,
				new ColorDockContent { Name = "Red", Color = "Red" },
				new ColorDockContent { Name = "Blue", Color = "Blue" }
			],
			[
				new ColorDockContent { Name = "Yellow", Color = "Yellow" },
				new ColorDockContent { Name = "Green", Color = "Green" }
			],
			[
				new ColorDockContent { Name = "Purple", Color = "Purple" },
				new ColorDockContent { Name = "Orange", Color = "Orange" }
			]);
		appService.SetActiveScreen(this);
	}

	public IReadOnlyList<Pod> Pods { get; private set; } = [];

	public TitleBar TitleBar { get; }

	public ToolbarGrid ToolbarGrid { get; }

	public Toolbar FileToolbar { get; }

	public Toolbar HelpToolbar { get; }

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