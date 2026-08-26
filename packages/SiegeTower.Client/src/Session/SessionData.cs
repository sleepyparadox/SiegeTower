using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.UX;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client;

public sealed class SessionData : IDataComponent
{
	public GraphCache Cache { get; } = new();
	public LoadingQueue LoadingQueue { get; } = new();
	public SessionContext Context { get; internal set; } = null!;
	public SessionServices Services { get; internal set; } = null!;
	public Action RequestRedraw { get; internal set; } = null!;
	public Action<string> NavigateTo { get; internal set; } = null!;
	public string BaseUri { get; internal set; } = string.Empty;
	public string ApiBaseUri { get; internal set; } = string.Empty;
	public string WorkspaceID { get; internal set; } = string.Empty;
	public BreadCrumb[] BreadCrumbs { get; internal set; } = [];
	public IScreenData ActiveScreen { get; internal set; } = null!;
	public IScreenData ActiveScreenData => ActiveScreen;
	public MenuItem[] BurgerMenu { get; internal set; } = [];
	public DragOperation DragOperation { get; } = new();
	public event EventHandler? RedrawRequested;

	public SessionData()
	{
		LoadingQueue.Changed += (_, _) => RequestRedrawEvent();
	}

	internal void RequestRedrawEvent() => RedrawRequested?.Invoke(this, EventArgs.Empty);
}
