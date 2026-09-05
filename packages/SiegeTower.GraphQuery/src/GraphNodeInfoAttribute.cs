namespace SiegeTower.GraphQuery;

public class GraphNodeInfoAttribute : Attribute
{
	public GraphNodeInfoAttribute(Func<IGraphIndex> newPrimaryIndex)
	{
		_newPrimaryIndex = newPrimaryIndex;
	}

	public IGraphIndex NewPrimaryIndex() => _newPrimaryIndex();

	Func<IGraphIndex> _newPrimaryIndex;
}