using SiegeTower.GraphQuery;

namespace SiegeTower.Data;

public record class WorkspaceRow(string Name, string Namespace) : IGraphNode
{
	public GraphCache Cache { get; set; } = null!;
}