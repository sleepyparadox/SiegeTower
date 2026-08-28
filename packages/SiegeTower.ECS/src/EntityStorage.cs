using System.ComponentModel;

public class EntityStorage
{
	public HashSet<Guid> Entities { get; } = new();
	public Dictionary<Type, Dictionary<Guid, IComponent>> Components { get; } = new();
}