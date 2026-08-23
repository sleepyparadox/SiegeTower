namespace SiegeTower.GraphQuery;

public class GraphIndexUniqueString<TNode> : GraphIndex<string, TNode> where TNode : IGraphNode
{
	Dictionary<string, TNode> _index = new();

	public override IEnumerable<TNode> Scan()
		=> _index.Select(pair => pair.Value);

	public override IEnumerable<TNode> Seek(string keyStartInclusive, string keyEndInclusive)
		=> _index.OrderBy(pair => pair.Key)
		.Where(pair =>
			string.CompareOrdinal(pair.Key, keyStartInclusive) >= 0
			&& string.CompareOrdinal(pair.Key, keyEndInclusive) <= 0)
		.Select(pair => pair.Value);

	public override IEnumerable<TNode> SeekTop1(string key)
	{
		if (_index.TryGetValue(key, out var node))
		{
			yield return node;
		}
	}
}