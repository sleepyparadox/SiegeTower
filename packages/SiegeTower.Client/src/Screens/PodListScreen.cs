using SiegeTower.Data;

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
		Left = CreateDock(
			PodListDockContent,
			new ColorDockContent { Color = "Red" },
			new ColorDockContent { Color = "Blue" });
		Center = CreateDock(
			new ColorDockContent { Color = "Yellow" },
			new ColorDockContent { Color = "Green" });
		Right = CreateDock(
			new ColorDockContent { Color = "Purple" },
			new ColorDockContent { Color = "Orange" });
		appService.SetActiveScreen(this);
	}

	public IReadOnlyList<Pod> Pods { get; private set; } = [];

	public PodListDockContent PodListDockContent { get; }

	public Dock Left { get; }

	public Dock Right { get; }

	public Dock Center { get; }

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

	private static Dock CreateDock(params object[] contents)
	{
		return new Dock
		{
			Contents = contents,
			ActiveContent = contents[0]
		};
	}
}