namespace SiegeTower.GraphQuery;

public interface IGraphIndex
{
	public Type NodeType { get; }

	public Type KeyType { get; }
}