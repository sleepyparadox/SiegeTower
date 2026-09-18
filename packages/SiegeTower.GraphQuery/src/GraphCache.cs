namespace SiegeTower.GraphQuery;

public class GraphCache
{
	public Dictionary<Type, IGraphIndex> PrimaryIndexes { get; } = new();
}
