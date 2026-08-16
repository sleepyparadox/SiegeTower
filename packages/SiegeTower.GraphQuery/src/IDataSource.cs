namespace SiegeTower.GraphQuery;

public interface IDataSource
{
	IEnumerable<T> Get<T>(Func<IEnumerable<T>, IEnumerable<T>> innerQuery) where T : IGraphNode;
}