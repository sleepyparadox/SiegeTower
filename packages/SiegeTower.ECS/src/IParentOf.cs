public interface IParentOf<T> where T : Component
{
	public Entity Entity { get; set; }
	public ComponentRefList<T> Children { get; set; }
}