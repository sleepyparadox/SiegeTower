public interface IChildOf<T> where T : Component
{
	public Entity Entity { get; set; }
	public ComponentRef<T> Parent { get; set; }
}