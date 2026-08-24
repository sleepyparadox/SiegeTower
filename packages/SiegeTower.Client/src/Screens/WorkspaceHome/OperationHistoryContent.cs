using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class OperationHistoryContent : IDockContent
{
	private readonly WorkspaceHomeScreen screen;

	public OperationHistoryContent(WorkspaceHomeScreen screen)
	{
		this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
	}

	public string Name => "Operation History";

	public Dock? Parent { get; set; }

	public IReadOnlyList<WorkspaceOperation> Operations => screen.UnitOfWork
		.GetPrimaryIndex<Guid, WorkspaceOperation>()
		.Scan()
		.OrderByDescending(operation => operation.CreatedAt)
		.ToArray();

	public IReadOnlyList<WorkspaceOperationLog> GetLogs(WorkspaceOperation operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		return screen.UnitOfWork
			.GetPrimaryIndex<OperationLogRowKey, WorkspaceOperationLog>()
			.Seek(
				new OperationLogRowKey(operation.ID, Guid.Empty),
				new OperationLogRowKey(operation.ID, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")))
			.OrderBy(log => log.CreatedAt)
			.ToArray();
	}
}
