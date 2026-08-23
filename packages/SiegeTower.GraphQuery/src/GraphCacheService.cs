using System.Reflection;

namespace SiegeTower.GraphQuery;

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