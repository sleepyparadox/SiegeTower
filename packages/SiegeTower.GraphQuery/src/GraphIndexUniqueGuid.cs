namespace SiegeTower.GraphQuery;

public class GraphIndexUniqueGuid<TNode> : GraphIndex<Guid, TNode> where TNode : IGraphNode
{
	Dictionary<Guid, TNode> _index = new();
	Func<TNode, Guid> _keySelector;

	public GraphIndexUniqueGuid(Func<TNode, Guid> keySelector)
	{
		_keySelector = keySelector;
	}

	public void Store(IEnumerable<TNode> nodes)
	{
		foreach (var node in nodes)
		{
			_index.Upsert(_keySelector(node), node);
		}
	}

	public override IEnumerable<TNode> Scan()
		=> _index.Select(pair => pair.Value);

	public override IEnumerable<TNode> Seek(Guid keyStartInclusive, Guid keyEndInclusive)
		=> _index.OrderBy(pair => pair.Key)
		.Where(pair => pair.Key >= keyEndInclusive && pair.Key <= keyEndInclusive)
		.Select(pair => pair.Value);

	public override IEnumerable<TNode> SeekTop1(Guid key)
	{
		if (_index.TryGetValue(key, out var node))
		{
			yield return node;
		}
	}
}