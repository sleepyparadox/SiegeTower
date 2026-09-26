using SiegeTower.GraphQuery;

namespace SiegeTower.Data;

[TaskRowInfo]
public record class TaskRow(Guid Id, string Name, string Description) : IGraphNode
{
	public GraphCache Cache { get; set; } = null!;
}

public class TaskRowInfoAttribute : GraphNodeInfoAttribute
{
	public TaskRowInfoAttribute() : base(newPrimaryIndex: () => new GraphIndexUniqueGuid<TaskRow>(node => node.Id))
	{
	}
}