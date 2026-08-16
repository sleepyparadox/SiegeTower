namespace SiegeTower.GraphQuery;

public class GraphCache
{
	public IDataSource? Source { get; set; }
}

public static class GraphService
{
	public static TNode[] Fetch<TNode>(GraphCache cache, Func<IEnumerable<TNode>, IEnumerable<TNode>> innerQuery) where TNode : IGraphNode
	{
		var results = cache.Source.Get<TNode>(innerQuery).ToArray();
		return results;
	}
}

