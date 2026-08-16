using SiegeTower.Client.Services.API;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListCreateContent : IDockContent
{
	#region IDockContent

	string IDockContent.Name => "Create";

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public WorkspaceListCreateContent(SessionContext sessionContext)
	{
		SessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
	}

	public SessionContext SessionContext { get; }

	public string WorkspaceName { get; set; } = "";

	public bool IsCreating { get; private set; }

	public async Task CreateAsync(CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(WorkspaceName) || IsCreating)
		{
			return;
		}

		IsCreating = true;
		try
		{
			await APIService.CreateWorkspace(SessionContext, WorkspaceName.Trim(), cancellationToken);
			WorkspaceName = string.Empty;
		}
		finally
		{
			IsCreating = false;
		}
	}
}
