using SiegeTower.GraphQuery;

namespace SiegeTower.Data;

[WorkspaceRowInfo]
public record class WorkspaceRow(string Name, string Namespace) : IGraphNode
{
	public GraphCache Cache { get; set; } = null!;
}

public class WorkspaceRowInfoAttribute : GraphNodeInfoAttribute
{
	public WorkspaceRowInfoAttribute() : base(newPrimaryIndex: () => new GraphIndexUniqueString<WorkspaceRow>(node => node.Namespace))
	{
	}
}