namespace SiegeTower.GraphQuery;

public static class GraphService
{
	public static void Add<TNode>(this GraphCache cache, IEnumerable<TNode> nodes)
		where TNode : IGraphNode
	{
		ArgumentNullException.ThrowIfNull(cache);
		ArgumentNullException.ThrowIfNull(nodes);

		var nodeIndexTypes = GetIndexTypes<TNode>();

		foreach (var node in nodes)
		{
			ArgumentNullException.ThrowIfNull(node);

			node.Cache = cache;
			cache.Nodes.Add(node);

			if (!cache.Indexes.TryGetValue(node, out var indexes))
			{
				indexes = [];
				cache.Indexes[node] = indexes;
			}

			foreach (var indexType in nodeIndexTypes)
			{
				if (!indexes.ContainsKey(indexType))
				{
					indexes[indexType] = CreateIndex(indexType);
				}
			}
		}
	}

	public static IEnumerable<T> Get<T>(GraphCache cache) where T : IGraphNode
	{
		ArgumentNullException.ThrowIfNull(cache);

		return cache.Nodes.OfType<T>();
	}

	private static IEnumerable<Type> GetIndexTypes<TNode>() where TNode : IGraphNode
	{
		return typeof(TNode)
			.GetCustomAttributes<GraphNodeIndexTypesAttribute>()
			.SelectMany(attribute => attribute.Types);
	}

	private static IGraphNodeIndex CreateIndex(Type indexType)
	{
		if (Activator.CreateInstance(indexType) is not IGraphNodeIndex index)
		{
			throw new InvalidOperationException($"Index type '{indexType}' must implement IGraphNodeIndex and have a public parameterless constructor.");
		}

		return index;
	}
}