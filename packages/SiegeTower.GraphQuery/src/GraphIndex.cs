namespace SiegeTower.GraphQuery;

public abstract class GraphIndex<TKey, TNode> : IGraphIndex where TNode : IGraphNode
{
	public Type NodeType => typeof(TNode);

	public Type KeyType => typeof(TKey);

	public abstract IEnumerable<TNode> Scan();

	public abstract IEnumerable<TNode> Seek(TKey keyStartInclusive, TKey keyEndInclusive);

	public abstract IEnumerable<TNode> SeekTop1(TKey key);
}