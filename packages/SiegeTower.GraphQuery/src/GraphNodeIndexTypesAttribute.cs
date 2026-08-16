namespace SiegeTower.GraphQuery;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class GraphNodeIndexTypesAttribute : Attribute
{
	public GraphNodeIndexTypesAttribute(Type primary, params Type[] others)
	{
		Types = [primary, .. others];
	}

	public IReadOnlyList<Type> Types { get; }
}