public interface IRequires<T> where T : Component
{
	public Entity Entity { get; set; }
}