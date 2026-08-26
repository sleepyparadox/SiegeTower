using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class OperationHistoryContent : IDockContent
{
	private readonly WorkspaceHomeScreenData data;

	public OperationHistoryContent(WorkspaceHomeScreenData data)
	{
		this.data = data ?? throw new ArgumentNullException(nameof(data));
	}

	public string Name => "Operation History";

	public Dock? Parent { get; set; }

	public IReadOnlyList<WorkspaceOperation> Operations => data.Cache
		.GetPrimaryIndex<Guid, WorkspaceOperation>()
		.Scan()
		.OrderByDescending(operation => operation.CreatedAt)
		.ToArray();

	public IReadOnlyList<WorkspaceOperationLog> GetLogs(WorkspaceOperation operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		return data.Cache
			.GetPrimaryIndex<OperationLogRowKey, WorkspaceOperationLog>()
			.Seek(
				new OperationLogRowKey(operation.ID, Guid.Empty),
				new OperationLogRowKey(operation.ID, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")))
			.OrderBy(log => log.CreatedAt)
			.ToArray();
	}
}
