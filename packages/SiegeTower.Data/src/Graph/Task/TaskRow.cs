using SiegeTower.GraphQuery;

namespace SiegeTower.Data;

public record class TaskRow(Guid Id, string Name, string Description) : IGraphNode
{
	public GraphCache Cache { get; set; } = null!;
}