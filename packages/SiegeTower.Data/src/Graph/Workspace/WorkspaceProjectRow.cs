using SiegeTower.GraphQuery;

namespace SiegeTower.Data;

[WorkspaceProjectRowInfo]
public record class WorkspaceProjectRow(string Namespace, string GitRepo) : IGraphNode
{
	public GraphCache Cache { get; set; } = null!;
}

public class WorkspaceProjectRowInfoAttribute : GraphNodeInfoAttribute
{
	public WorkspaceProjectRowInfoAttribute() : base(newPrimaryIndex: () => new GraphIndexUniqueString<WorkspaceProjectRow>(node => node.Namespace))
	{
	}
}