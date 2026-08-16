namespace SiegeTower.GraphQuery;

public interface IDataSource
{
	IEnumerable<T> Get<T>() where T : IGraphNode;
}