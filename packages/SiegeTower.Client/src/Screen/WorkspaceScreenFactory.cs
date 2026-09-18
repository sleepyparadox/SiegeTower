using System.Text.Json;
using SiegeTower.Data;
using SiegeTower.Data.Graph.File;

namespace SiegeTower.Client;

public static class WorkspaceScreenFactory
{
	public static Screen CreateWorkspaceHomeScreen(Session session, string workspacePath)
	{
		var screen = CreateScreen(session, "Workspace", workspacePath);
		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var workspaceNavToolbar = WorkspaceNavToolbar(screen, session, workspacePath);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([workspaceNavToolbar, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		var settingsWindow = AddDockWindow(screen, root, "Workspace Settings", string.Empty);
		AddWorkspaceSettingsLayout(screen, session, settingsWindow, workspacePath);
		var chatHistoryWindow = AddDockWindow(screen, root, "Chat History", string.Empty);
		var chatHistoryLayout = AddChatHistoryLayout(screen, chatHistoryWindow);
		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadChatHistory(session, workspacePath, chatHistoryLayout)));

		var inputContainer = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		inputContainer.IsFixedHeight = true;
		inputContainer.HeightInGridUnits = 6;
		root.AttachChild<DockContainer, DockLayoutNode>(inputContainer);
		var inputWindow = AddDockWindow(screen, inputContainer, "Chat Input", string.Empty);
		AddChatInputLayout(screen, inputWindow, workspacePath);
		return screen;
	}

	static async Task LoadChatHistory(Session session, string workspacePath, ControlLayoutNode chatHistoryLayout)
	{
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		var operations = await WorkspaceAPISystem.Get<OperationRow[]>(session, workspaceId, "operation");
		var logs = await WorkspaceAPISystem.Get<OperationLogRow[]>(session, workspaceId, "operation/all/log");
		if (operations is null || logs is null)
		{
			return;
		}

		ClearChatHistory(chatHistoryLayout);
		foreach (var operation in operations.OrderBy(operation => operation.CreatedAt))
		{
			AddHistoryLabel(chatHistoryLayout, $"User: {operation.Operation.Prompt?.Prompt ?? "Operation without a prompt."}");
			foreach (var log in logs
				.Where(log => log.Operation_ID == operation.ID)
				.OrderBy(log => log.CreatedAt))
			{
				AddHistoryLabel(chatHistoryLayout, $"Assistant: {log.Message}");
			}
		}

		session.Redraw();
	}

	static ControlLayoutNode AddChatHistoryLayout(Screen screen, DockWindow window)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		layout.AttachChild(root);
		return root;
	}

	static void AddHistoryLabel(ControlLayoutNode historyLayout, string value)
	{
		var control = historyLayout.Entity.EntityStorage.NewEntity<ControlLayoutControl>();
		control.AddComponent(entity => new LabelControl(entity, value));
		historyLayout.AttachChild(control);
	}

	static void ClearChatHistory(ControlLayoutNode historyLayout)
	{
		foreach (var control in ((IParentOf<ControlLayoutControl>)historyLayout).Children.Values.ToArray())
		{
			control.Entity.TryDeleteEntity();
		}

		((IParentOf<ControlLayoutControl>)historyLayout).Children.Values.Clear();
	}

	public static Screen CreateWorkspaceFilesScreen(Session session, string workspacePath)
	{
		var screen = CreateScreen(session, "Workspace Files", workspacePath, "Files");
		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var workspaceNavToolbar = WorkspaceNavToolbar(screen, session, workspacePath);
		var toolbarLayout = AddWorkspaceToolbar(screen);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([workspaceNavToolbar, toolbarLayout, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Horizontal));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		var filesContainer = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		filesContainer.IsFixedWidth = true;
		filesContainer.WidthInGridUnits = 10;
		root.AttachChild<DockContainer, DockLayoutNode>(filesContainer);
		var filesWindow = AddDockWindow(screen, filesContainer, "Files", string.Empty);
		filesWindow.AddComponent<ControlLayout>();
		filesWindow.AddComponent<DockWindowControlLayout>();
		var fileTree = AddFileTreeControlLayout(screen, filesWindow);
		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadFiles(session, screen, workspacePath, root, fileTree)));
		return screen;
	}

	public static Screen CreateWorkspaceGitScreen(Session session, string workspacePath)
	{
		var screen = CreateScreen(session, "Workspace Git", workspacePath, "Git");
		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var workspaceNavToolbar = WorkspaceNavToolbar(screen, session, workspacePath);
		var gitToolbar = AddGitToolbar(screen, workspacePath);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([workspaceNavToolbar, gitToolbar, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Horizontal));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		var gitContainer = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		root.AttachChild<DockContainer, DockLayoutNode>(gitContainer);
		var reposWindow = AddDockWindow(screen, gitContainer, "Repositories", string.Empty);
		var reposLayout = reposWindow.AddComponent<ControlLayout>();
		reposWindow.AddComponent<DockWindowControlLayout>();
		var repoList = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		reposLayout.AttachChild(repoList);
		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadGitRepos(session, screen, workspacePath, repoList)));
		AddDockWindow(screen, root, "Commits", "main\n  Initial commit\n  Add workspace files\n  Update agent prompt");
		AddDockWindow(screen, root, "Git Details", "Select a commit or changed file to inspect it.");
		return screen;
	}

	static Screen CreateScreen(Session session, string title, string workspacePath, string? childBreadcrumb = null)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(workspacePath);

		var screen = new Screen(session, title);
		var titleLayout = AddTitleLayout(screen, title);
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", 1);
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		screen.AddNewBreadCrumbEntity(titleLayout, workspaceId, workspacePath, 2);
		if (childBreadcrumb is not null)
		{
			screen.AddNewBreadCrumbEntity(titleLayout, childBreadcrumb, $"{workspacePath}/{childBreadcrumb.ToLowerInvariant().Replace(' ', '-')}", 3);
		}

		return screen;
	}

	static ToolbarLayout WorkspaceNavToolbar(Screen screen, Session session, string workspacePath)
		=> screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Workspace", session => session.HandleEvent(new NavigationEvent(workspacePath)))),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Files", session => session.HandleEvent(new NavigationEvent($"{workspacePath}/files")))),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Git", session => session.HandleEvent(new NavigationEvent($"{workspacePath}/git"))))
			])
		]);

	static ToolbarLayout AddWorkspaceToolbar(Screen screen)
		=> screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Workspace")),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => new ComboBoxControl(entity, "Development"))
			])
		]);

	static ToolbarLayout AddGitToolbar(Screen screen, string workspacePath)
	{
		TextInputControl? cloneRepo = null;
		TextInputControl? clonePath = null;
		ComboBoxControl? pushRepo = null;
		TextInputControl? pushBranch = null;
		ComboBoxControl? branchRepo = null;
		TextInputControl? newBranch = null;
		return screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Git clone")),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Repository")),
				toolbar.AddToolbarControl<TextInputControl>(entity => { cloneRepo = new TextInputControl(entity, string.Empty); return cloneRepo; }),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Local path")),
				toolbar.AddToolbarControl<TextInputControl>(entity => { clonePath = new TextInputControl(entity, string.Empty); return clonePath; }),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Clone", session =>
				{
					screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => SubmitGitClone(session, workspacePath, cloneRepo!, clonePath!)));
				}))
			]),
			layout.AddToolbar(1).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Git push")),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Repository")),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => { pushRepo = new ComboBoxControl(entity, string.Empty); return pushRepo; }),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Branch")),
				toolbar.AddToolbarControl<TextInputControl>(entity => { pushBranch = new TextInputControl(entity, "main"); return pushBranch; }),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Push", session =>
					screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => SubmitGitPush(session, workspacePath, pushRepo!, pushBranch!)))))
			]),
			layout.AddToolbar(2).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Git create branch")),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Repository")),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => { branchRepo = new ComboBoxControl(entity, string.Empty); return branchRepo; }),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Branch")),
				toolbar.AddToolbarControl<TextInputControl>(entity => { newBranch = new TextInputControl(entity, string.Empty); return newBranch; }),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Create", session =>
					screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => SubmitGitBranch(session, workspacePath, branchRepo!, newBranch!)))))
			])
		]);
	}

	static async Task LoadGitRepos(Session session, Screen screen, string workspacePath, ControlLayoutNode repoList)
	{
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		var repos = await WorkspaceAPISystem.Get<GitRepoRow[]>(session, workspaceId, "git/repo");
		if (repos is null)
		{
			return;
		}

		foreach (var repo in repos)
		{
			var control = screen.NewEntity<ControlLayoutControl>();
			control.AddComponent(entity => new LabelControl(entity, string.IsNullOrEmpty(repo.Repo) ? repo.LocalPath : $"{repo.LocalPath} ({repo.Repo})"));
			repoList.AttachChild(control);
		}

		var combos = screen.SelectComponents<ComboBoxControl>().ToArray();
		foreach (var combo in combos)
		{
			combo.Items = repos.Select(repo => repo.LocalPath).ToList();
			combo.Value = combo.Items.FirstOrDefault() ?? string.Empty;
		}
		session.Redraw();
	}

	static async Task SubmitGitClone(Session session, string workspacePath, TextInputControl repo, TextInputControl localPath)
	{
		if (string.IsNullOrWhiteSpace(repo.Value) || string.IsNullOrWhiteSpace(localPath.Value)) return;
		await SubmitOperation(session, workspacePath, new Operation { GitClone = new GitCloneOperation { Repo = repo.Value.Trim(), LocalPath = localPath.Value.Trim() } });
	}

	static async Task SubmitGitPush(Session session, string workspacePath, ComboBoxControl repo, TextInputControl branch)
	{
		if (string.IsNullOrWhiteSpace(repo.Value) || string.IsNullOrWhiteSpace(branch.Value)) return;
		await SubmitOperation(session, workspacePath, new Operation { GitPushOperation = new GitPushOperation { LocalPath = repo.Value, Branch = branch.Value.Trim() } });
	}

	static async Task SubmitGitBranch(Session session, string workspacePath, ComboBoxControl repo, TextInputControl branch)
	{
		if (string.IsNullOrWhiteSpace(repo.Value) || string.IsNullOrWhiteSpace(branch.Value)) return;
		await SubmitOperation(session, workspacePath, new Operation { GitCreateBranch = new GitCreateBranchOperation { LocalPath = repo.Value, Branch = branch.Value.Trim() } });
	}

	static async Task SubmitOperation(Session session, string workspacePath, Operation operation)
	{
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		await WorkspaceAPISystem.Post<OperationRow, OperationRow>(session, workspaceId, "operation", new OperationRow
		{
			ID = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Operation = operation
		});
		session.Redraw();
	}

	static void AddWorkspaceSettingsLayout(Screen screen, Session session, DockWindow window, string workspacePath)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
		layout.AttachChild(root);
		var tokenLabel = screen.NewEntity<ControlLayoutControl>();
		tokenLabel.AddComponent(entity => new LabelControl(entity, "Git access token"));
		root.AttachChild(tokenLabel);
		var tokenControl = screen.NewEntity<ControlLayoutControl>();
		var token = tokenControl.AddComponent(entity => new TextInputControl(entity, string.Empty));
		root.AttachChild(tokenControl);
		var save = screen.NewEntity<ControlLayoutControl>();
		save.AddComponent(entity => new ButtonControl(entity, "Save", session => screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => SaveWorkspaceSettings(session, workspacePath, token)))));
		root.AttachChild(save);
		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadWorkspaceSettings(session, workspacePath, token)));
	}

	static async Task LoadWorkspaceSettings(Session session, string workspacePath, TextInputControl token)
	{
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		var settings = await WorkspaceAPISystem.Get<WorkspaceSettings>(session, workspaceId, "workspace/settings");
		if (settings is not null)
		{
			var accessToken = ParseGithubAccessToken(settings.GitAccessToken ?? string.Empty);
			token.Value = accessToken;
			if (!string.Equals(settings.GitAccessToken, accessToken, StringComparison.Ordinal))
			{
				await WorkspaceAPISystem.Post<WorkspaceSettings, WorkspaceSettings>(session, workspaceId, "workspace/settings", new WorkspaceSettings { GitAccessToken = accessToken });
			}
		}
		session.Redraw();
	}

	static async Task SaveWorkspaceSettings(Session session, string workspacePath, TextInputControl token)
	{
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		var accessToken = ParseGithubAccessToken(token.Value);
		token.Value = accessToken;
		await WorkspaceAPISystem.Post<WorkspaceSettings, WorkspaceSettings>(session, workspaceId, "workspace/settings", new WorkspaceSettings { GitAccessToken = accessToken });
		session.Redraw();
	}

	static string ParseGithubAccessToken(string value)
	{
		var trimmedValue = value.Trim();
		if (trimmedValue.StartsWith('{'))
		{
			try
			{
				using var document = JsonDocument.Parse(trimmedValue);
				if (document.RootElement.TryGetProperty("token", out var token)
					&& token.ValueKind == JsonValueKind.String
					&& !string.IsNullOrWhiteSpace(token.GetString()))
				{
					return token.GetString()!.Trim();
				}
			}
			catch (JsonException)
			{
			}
		}

		return trimmedValue;
	}

	static void AddChatInputLayout(Screen screen, DockWindow window, string workspacePath)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity<ControlLayoutNode>(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
		layout.AttachChild(root);
		var input = screen.NewEntity<ControlLayoutControl>();
		var textInput = input.AddComponent(entity => new TextInputControl(entity, string.Empty));
		root.AttachChild(input);
		var send = screen.NewEntity<ControlLayoutControl>();
		send.AddComponent(entity => new ButtonControl(entity, "Send", session =>
			screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => SendChat(session, workspacePath, textInput)))));
		root.AttachChild(send);
	}

	static async Task SendChat(Session session, string workspacePath, TextInputControl textInput)
	{
		if (string.IsNullOrWhiteSpace(textInput.Value))
		{
			return;
		}

		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		var operation = new OperationRow
		{
			ID = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			Operation = new Operation
			{
				Prompt = new PromptOperation { Prompt = textInput.Value }
			}
		};

		var response = await WorkspaceAPISystem.Post<OperationRow, OperationRow>(session, workspaceId, "operation", operation);
		if (response is not null)
		{
			textInput.Value = string.Empty;
			session.Redraw();
		}
	}

	static async Task LoadFiles(Session session, Screen screen, string workspacePath, DockContainer root, TreeControl fileTree)
	{
		var workspaceId = workspacePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
		var files = await WorkspaceAPISystem.Get<FileRow[]>(session, workspaceId, "file");
		if (files is null)
		{
			return;
		}

		fileTree.Children.Values.Clear();
		foreach (var file in files.OrderBy(file => file.Path, StringComparer.Ordinal))
		{
			AddFileTreePath(fileTree.Entity.EntityStorage, fileTree, file.Path,
				session => screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity,
					() => OpenFilePreview(session, screen, root, workspaceId, file.Path))));
		}

		session.Redraw();
	}

	static void AddFileTreePath(EntityStorage storage, TreeControl fileTree, string path, Action<Session> onClick)
	{
		var segments = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
		TreeNode? parent = null;
		var currentPath = string.Empty;

		for (var index = 0; index < segments.Length; index++)
		{
			var segment = segments[index];
			currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}/{segment}";
			var isFile = index == segments.Length - 1;
			var node = parent is null
				? fileTree.Children.Values.FirstOrDefault(child => child.Text == segment)
				: ((IParentOf<TreeNode>)parent).Children.Values.FirstOrDefault(child => child.Text == segment);

			if (node is null)
			{
				node = storage.NewEntity(entity => new TreeNode(entity, segment, isFile ? TreeNodeIcon.File : TreeNodeIcon.Folder));
				node.Path = isFile ? path : null;
				node.OnClick = isFile ? onClick : null;
				if (parent is null)
				{
					fileTree.AttachChild(node);
				}
				else
				{
					parent.AttachChild(node);
				}
			}

			if (!isFile)
			{
				node.IsExpanded = true;
			}
			parent = node;
		}
	}

	static async Task OpenFilePreview(Session session, Screen screen, DockContainer root, string workspaceId, string path)
	{
		var files = await WorkspaceAPISystem.Get<FileRow[]>(session, workspaceId, "file?contents=true");
		var file = files?.SingleOrDefault(file => string.Equals(file.Path, path, StringComparison.Ordinal));
		if (file is null)
		{
			return;
		}

		var extension = Path.GetExtension(file.Path).ToLowerInvariant();
		var mimeType = extension switch
		{
			".png" => "image/png",
			".jpg" or ".jpeg" => "image/jpeg",
			".gif" => "image/gif",
			".webp" => "image/webp",
			".bmp" => "image/bmp",
			".svg" => "image/svg+xml",
			_ => "text/plain"
		};
		var preview = screen.SelectComponents<FilePreviewComponent>().SingleOrDefault();
		var isImage = mimeType.StartsWith("image/", StringComparison.Ordinal);
		if (preview is null)
		{
			var window = AddDockWindow(screen, root, file.Path, string.Empty);
			preview = window.AddComponent(entity => new FilePreviewComponent(entity, file.Path, file.Contents ?? string.Empty, isImage, mimeType));
		}
		else
		{
			preview.FilePath = file.Path;
			preview.Contents = file.Contents ?? string.Empty;
			preview.IsImage = isImage;
			preview.MimeType = mimeType;
		}

		var previewWindow = preview.GetComponent<DockWindow>();
		previewWindow.Name = file.Path;
		if (previewWindow.Parent.Get() is DockWindowGroup previewGroup)
		{
			previewGroup.ActiveWindow = previewWindow;
		}

		session.Redraw();
	}

	static TreeControl AddFileTreeControlLayout(Screen screen, DockWindow window)
	{
		var layout = window.Entity.EntityStorage.SelectComponents<ControlLayout>().Single(layout => layout.Entity.ID == window.Entity.ID);
		var root = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		layout.AttachChild(root);
		var treeControl = screen.NewEntity<ControlLayoutControl>();
		var tree = treeControl.AddComponent<TreeControl>();
		root.AttachChild(treeControl);
		return tree;
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

	static DockWindow AddDockWindow(Screen screen, DockContainer container, string title, string content)
	{
		var group = screen.NewEntity<DockWindowGroup>();
		container.AttachChild<DockContainer, DockLayoutNode>(group);
		var window = screen.NewEntity(entity => new DockWindow(entity, title, content));
		group.AttachChild(window);
		group.ActiveWindow = window;
		return window;
	}
}
