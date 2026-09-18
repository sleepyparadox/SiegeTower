using System.Text.Json.Serialization;
using SiegeTower.Data.Ollama;

namespace SiegeTower.Client;

public static class AgentScreenFactory
{
	public static Screen CreateAgentScreen(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);

		var screen = new Screen(session, "Agent");
		var titleLayout = screen.NewEntity<TitleLayout>(entity => new TitleLayout(entity, "Agent"));
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Agent", "/agent", 1);
		var screenLayout = screen.NewEntity<ScreenLayout>();
		var toolbar = CommonScreenFactory.AddTopLevelToolbar(screen, session);
		screenLayout.AttachChild<ScreenLayout, ScreenLayoutChild>(titleLayout);
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([toolbar]);

		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChild<ScreenLayout, ScreenLayoutChild>(dockingLayout);
		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);

		var modelsWindow = AddDockWindow(screen, root, "Models");
		var modelsLayout = modelsWindow.AddComponent<ControlLayout>();
		modelsWindow.AddComponent<DockWindowControlLayout>();
		var modelList = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		modelsLayout.AttachChild(modelList);

		var downloadWindow = AddDockWindow(screen, root, "Download Model");
		AddDownloadLayout(screen, downloadWindow, modelList);
		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadModels(session, screen, modelList)));
		return screen;
	}

	static async Task LoadModels(Session session, Screen screen, ControlLayoutNode modelList)
	{
		var response = await HttpSystem.Get<OllamaTagsResponse>(session, "/ollama/api/tags");
		if (response is null)
		{
			return;
		}

		ClearModelList(modelList);
		foreach (var model in response.Models.OrderBy(model => model.Name, StringComparer.OrdinalIgnoreCase))
		{
			var control = screen.NewEntity<ControlLayoutControl>();
			control.AddComponent(entity => new LabelControl(entity, model.Name));
			modelList.AttachChild(control);
		}

		session.Redraw();
	}

	static void AddDownloadLayout(Screen screen, DockWindow window, ControlLayoutNode modelList)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
		layout.AttachChild(root);
		var input = screen.NewEntity<ControlLayoutControl>();
		var modelInput = input.AddComponent(entity => new TextInputControl(entity, string.Empty));
		root.AttachChild(input);
		var button = screen.NewEntity<ControlLayoutControl>();
		button.AddComponent(entity => new ButtonControl(entity, "Download", session =>
			screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => DownloadModel(session, screen, modelInput, modelList)))));
		root.AttachChild(button);
	}

	static async Task DownloadModel(Session session, Screen screen, TextInputControl modelInput, ControlLayoutNode modelList)
	{
		if (string.IsNullOrWhiteSpace(modelInput.Value))
		{
			return;
		}

		var response = await HttpSystem.Post<ModelPullRequest, ModelPullResponse>(session, "/ollama/api/pull",
			new ModelPullRequest { Name = modelInput.Value.Trim(), Stream = false });
		if (response?.Status == "success")
		{
			modelInput.Value = string.Empty;
			await LoadModels(session, screen, modelList);
		}
	}

	static void ClearModelList(ControlLayoutNode modelList)
	{
		foreach (var control in ((IParentOf<ControlLayoutControl>)modelList).Children.Values.ToArray())
		{
			control.Entity.TryDeleteEntity();
		}

		((IParentOf<ControlLayoutControl>)modelList).Children.Values.Clear();
	}

	static DockWindow AddDockWindow(Screen screen, DockContainer container, string title)
	{
		var group = screen.NewEntity<DockWindowGroup>();
		container.AttachChild<DockContainer, DockLayoutNode>(group);
		var window = screen.NewEntity(entity => new DockWindow(entity, title, string.Empty));
		group.AttachChild(window);
		group.ActiveWindow = window;
		return window;
	}

	private sealed class ModelPullRequest
	{
		public string Name { get; set; } = string.Empty;
		public bool Stream { get; set; }
	}

	private sealed class ModelPullResponse
	{
		public string Status { get; set; } = string.Empty;
	}
}