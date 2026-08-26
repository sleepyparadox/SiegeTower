using SiegeTower.Client.UX;
using SiegeTower.Data;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class OperationHistoryContent : IDockContent
{
	private readonly WorkspaceHomeScreenData data;

	public OperationHistoryContent(WorkspaceHomeScreenData data)
	{
		this.data = data ?? throw new ArgumentNullException(nameof(data));
	}

	public string Name => "Operation History";

	public WorkspaceHomeScreenData Data => data;

	public Dock? Parent { get; set; }
}
