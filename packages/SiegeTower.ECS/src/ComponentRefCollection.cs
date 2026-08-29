/*
	Exists for serialization

	eg:
	[
		guid EntityID,
		guid EntityID,
		guid EntityID,
		guid EntityID,
		guid EntityID,
	]
*/


public class ComponentRefList<T> where T : Component
{
	public List<T> Values { get; set; } = new();

	public void Add(T value) => Values.Add(value);

	public bool Remove(T value) => Values.Remove(value);

	public int IndexOf(T value) => Values.IndexOf(value);
}