using System.Reflection;

namespace SiegeTower.GraphQuery;

public class GraphCache
{
	public Dictionary<Type, IGraphIndex> PrimaryIndexes { get; } = new();
}

public interface IGraphIndex
{
	public Type NodeType { get;}

	public Type KeyType { get;}
}

public abstract class GraphIndex<TKey, TNode> : IGraphIndex where TNode : IGraphNode
{
	public Type NodeType => typeof(TNode);

	public Type KeyType => typeof(TKey);

	public abstract IEnumerable<TNode> Scan();

	public abstract IEnumerable<TNode> Seek(TKey keyStartInclusive, TKey keyEndInclusive);

	public abstract IEnumerable<TNode> SeekTop1(TKey key);

}

public static class DictionaryExtensions
{
	public static void Upsert<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
		}
		else
		{
			dictionary.Add(key, value);
		}
	}
}

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

public static class GraphCacheService
{
	public static GraphIndex<TKey, TNode> GetPrimaryIndex<TKey, TNode>(this GraphCache cache) where TNode : IGraphNode
	{
		var nodeType = typeof(TNode);
		if (!cache.PrimaryIndexes.TryGetValue(nodeType, out var primaryIndex))
		{
			var nodeInfo = nodeType.GetCustomAttribute<GraphNodeInfoAttribute>();
			primaryIndex = nodeInfo!.NewPrimaryIndex();
			cache.PrimaryIndexes.Add(nodeType, primaryIndex);
		}

		return (GraphIndex<TKey, TNode>)primaryIndex;
	}
}

public class GraphNodeInfoAttribute : Attribute
{
	public GraphNodeInfoAttribute(Func<IGraphIndex> newPrimaryIndex)
	{
		_newPrimaryIndex = newPrimaryIndex;
	}

	public IGraphIndex NewPrimaryIndex() => _newPrimaryIndex();

	Func<IGraphIndex> _newPrimaryIndex;
}



