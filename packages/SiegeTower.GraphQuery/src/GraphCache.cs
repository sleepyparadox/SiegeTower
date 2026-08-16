namespace SiegeTower.GraphQuery;

public class GraphCache
{
	public IDataSource? Source { get; set; }

	public List<IGraphNode> Nodes { get; } = [];

	public Dictionary<IGraphNode, Dictionary<Type, IGraphNodeIndex>> Indexes { get; } = [];
}