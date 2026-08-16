namespace SiegeTower.GraphQuery;

public class GraphIndexMany<TKey, TNode> : IGraphNodeIndex
{
	public Dictionary<TKey, List<TNode>> Items { get; } = [];
}