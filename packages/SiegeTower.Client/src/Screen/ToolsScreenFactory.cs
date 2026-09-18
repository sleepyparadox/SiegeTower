namespace SiegeTower.Client;

public static class ToolsScreenFactory
{
	const string AppIdKey = "siegetower.tools.github.app-id";
	const string InstallationIdKey = "siegetower.tools.github.installation-id";
	const string PrivateKeyKey = "siegetower.tools.github.private-key";

	public static Screen CreateToolsScreen(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);

		var screen = new Screen(session, "Tools");
		var titleLayout = AddTitleLayout(screen, "Tools");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Tools", "/tools", 1);

		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var toolbar = CommonScreenFactory.AddTopLevelToolbar(screen, session);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([toolbar, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		var formWindow = AddDockWindow(screen, root, "GitHub Access Token Generation", string.Empty);
		var controls = AddGithubAccessTokenForm(screen, formWindow);

		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadTools(session, controls)));
		return screen;
	}

	static async Task LoadTools(Session session, GithubAccessTokenControls controls)
	{
		controls.AppId.Value = await LocalStorageService.Get(session, AppIdKey) ?? string.Empty;
		controls.InstallationId.Value = await LocalStorageService.Get(session, InstallationIdKey) ?? string.Empty;
		controls.PrivateKey.Value = await LocalStorageService.Get(session, PrivateKeyKey) ?? string.Empty;

		session.Redraw();
	}

	static GithubAccessTokenControls AddGithubAccessTokenForm(Screen screen, DockWindow window)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		layout.AttachChild(root);

		var controls = new GithubAccessTokenControls(
			AddLabeledInput(screen, root, "App ID"),
			AddLabeledInput(screen, root, "Installation ID"),
			AddLabeledInput(screen, root, "Private key"));
		var generate = screen.NewEntity<ControlLayoutControl>();
		generate.AddComponent(entity => new ButtonControl(entity, "Generate", session =>
			screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => GenerateGithubAccessToken(session, controls)))));
		root.AttachChild(generate);
		return controls;
	}

	static TextInputControl AddLabeledInput(Screen screen, ControlLayoutNode root, string label)
	{
		var row = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
		root.AttachChild(row);
		var labelControl = screen.NewEntity<ControlLayoutControl>();
		labelControl.AddComponent(entity => new LabelControl(entity, label));
		row.AttachChild(labelControl);
		var inputControl = screen.NewEntity<ControlLayoutControl>();
		var input = inputControl.AddComponent(entity => new TextInputControl(entity, string.Empty));
		row.AttachChild(inputControl);
		return input;
	}

	static async Task GenerateGithubAccessToken(Session session, GithubAccessTokenControls controls)
	{
		if (string.IsNullOrWhiteSpace(controls.AppId.Value)
			|| string.IsNullOrWhiteSpace(controls.InstallationId.Value)
			|| string.IsNullOrWhiteSpace(controls.PrivateKey.Value))
		{
			return;
		}

		await LocalStorageService.Set(session, AppIdKey, controls.AppId.Value);
		await LocalStorageService.Set(session, InstallationIdKey, controls.InstallationId.Value);
		await LocalStorageService.Set(session, PrivateKeyKey, controls.PrivateKey.Value);

		await session.JSRuntime.InvokeAsync<object?>("siegeTower.submitGithubAccessToken", [
			controls.AppId.Value,
			controls.InstallationId.Value,
			controls.PrivateKey.Value]);
	}

	static TitleLayout AddTitleLayout(Screen screen, string title)
	{
		var screenLayout = screen.SelectComponents<ScreenLayout>().SingleOrDefault();
		if (screenLayout is null)
		{
			screenLayout = screen.NewEntity().AddComponent<ScreenLayout>();
		}

		var titleLayout = screen.NewEntity().AddComponent(entity => new TitleLayout(entity, title));
		ParentingSystem.AttachParentChild<ScreenLayout, ScreenLayoutChild>(screenLayout, titleLayout);
		return titleLayout;
	}

	static DockWindow AddDockWindow(Screen screen, DockContainer container, string title, string contents)
	{
		var group = screen.NewEntity<DockWindowGroup>();
		container.AttachChild<DockContainer, DockLayoutNode>(group);
		var window = screen.NewEntity(entity => new DockWindow(entity, title, contents));
		group.AttachChild(window);
		group.ActiveWindow = window;
		return window;
	}

	sealed record GithubAccessTokenControls(TextInputControl AppId, TextInputControl InstallationId, TextInputControl PrivateKey);

}
