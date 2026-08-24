using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class GitCloneOperationDockContent : IDockContent
{
	public string Name => "Git Clone";

	public Dock? Parent { get; set; }
}

public sealed class GitCreateBranchOperationDockContent : IDockContent
{
	public string Name => "Create Branch";

	public Dock? Parent { get; set; }
}

public sealed class GitPushOperationDockContent : IDockContent
{
	public string Name => "Git Push";

	public Dock? Parent { get; set; }
}

public sealed class GitCommitOperationDockContent : IDockContent
{
	public string Name => "Git Commit";

	public Dock? Parent { get; set; }
}

public sealed class PromptOperationDockContent : IDockContent
{
	public string Name => "Prompt";

	public Dock? Parent { get; set; }
}
