namespace SiegeTower.GraphQuery;

public class GraphIndexUnique<TKey, TNode> : IGraphNodeIndex
{
	public Dictionary<TKey, TNode> Items { get; } = [];
}