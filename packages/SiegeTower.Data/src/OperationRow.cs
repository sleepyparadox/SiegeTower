using System.Text.Json.Serialization;
using SiegeTower.GraphQuery;

namespace SiegeTower.Data;

public class WorkspaceSettings
{
	public string? GitAccessToken { get; set; }

	public string? GitBranchName { get; set; }

	public string? GitPR { get; set; }
}

[OperationRowInfo]
public class OperationRow : IGraphNode
{
	public Guid ID { get; set; }

	public DateTime CreatedAt { get; set; }

	public Operation Operation { get; set; } = new();

	[JsonIgnore]
	public GraphCache Cache { get; set; } = null!;
}

public sealed class Operation
{
	public GitCloneOperation? GitClone { get; set; }

	public GitCreateBranchOperation? GitCreateBranch { get; set; }

	public GitPushOperation? GitPushOperation { get; set; }

	public GitCommitOperation? GitCommitOperation { get; set; }

	public PromptOperation? Prompt { get; set; }
}

public sealed class GitCloneOperation
{
	public string LocalPath { get; set; } = string.Empty;

	public string Repo { get; set; } = string.Empty;

	public string Branch { get; set; } = string.Empty;
}

public sealed class GitCreateBranchOperation
{
	public string Branch { get; set; } = string.Empty;
}

public sealed class GitPushOperation
{
	public string Branch { get; set; } = string.Empty;
}

public sealed class GitCommitOperation
{
	public string Message { get; set; } = string.Empty;
}

public sealed class PromptOperation
{
	public string Prompt { get; set; } = string.Empty;
}

[OperationLogRowInfo]
public class OperationLogRow : IGraphNode
{
	public Guid ID { get; set; }

	public Guid Operation_ID { get; set; }

	public DateTime CreatedAt { get; set; }

	public string Message { get; set; } = string.Empty;

	[JsonIgnore]
	public GraphCache Cache { get; set; } = null!;
}

[WorkspaceOperationInfo]
public class WorkspaceOperation : OperationRow
{
}

[WorkspaceOperationLogInfo]
public class WorkspaceOperationLog : OperationLogRow
{
}

public readonly record struct OperationLogRowKey(Guid Operation_ID, Guid ID);

public sealed class OperationRowInfoAttribute : GraphNodeInfoAttribute
{
	public OperationRowInfoAttribute() : base(() => new GraphIndexUniqueGuid<OperationRow>(node => node.ID))
	{
	}
}

public sealed class OperationLogRowInfoAttribute : GraphNodeInfoAttribute
{
	public OperationLogRowInfoAttribute() : base(() => new GraphIndexUniqueOperationIDSelfID<OperationLogRow>())
	{
	}
}

public sealed class WorkspaceOperationInfoAttribute : GraphNodeInfoAttribute
{
	public WorkspaceOperationInfoAttribute() : base(() => new GraphIndexUniqueGuid<WorkspaceOperation>(node => node.ID))
	{
	}
}

public sealed class WorkspaceOperationLogInfoAttribute : GraphNodeInfoAttribute
{
	public WorkspaceOperationLogInfoAttribute() : base(() => new GraphIndexUniqueOperationIDSelfID<WorkspaceOperationLog>())
	{
	}
}

public sealed class GraphIndexUniqueOperationIDSelfID<TNode> : GraphIndex<OperationLogRowKey, TNode>
	where TNode : OperationLogRow
{
	private readonly Dictionary<OperationLogRowKey, TNode> index = new();

	public void Store(IEnumerable<TNode> nodes)
	{
		foreach (var node in nodes)
		{
			index[new OperationLogRowKey(node.Operation_ID, node.ID)] = node;
		}
	}

	public override IEnumerable<TNode> Scan()
		=> index.Values;

	public override IEnumerable<TNode> Seek(OperationLogRowKey keyStartInclusive, OperationLogRowKey keyEndInclusive)
		=> index.OrderBy(pair => pair.Key.Operation_ID).ThenBy(pair => pair.Key.ID)
			.Where(pair => pair.Key.Operation_ID >= keyStartInclusive.Operation_ID
				&& pair.Key.Operation_ID <= keyEndInclusive.Operation_ID
				&& pair.Key.ID >= keyStartInclusive.ID
				&& pair.Key.ID <= keyEndInclusive.ID)
			.Select(pair => pair.Value);

	public override IEnumerable<TNode> SeekTop1(OperationLogRowKey key)
	{
		if (index.TryGetValue(key, out var node))
		{
			yield return node;
		}
	}
}