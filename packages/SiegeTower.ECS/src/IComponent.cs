public interface IComponent
{
	public Guid EntityID { get; set; }
	
	public EntityStorage EntityStorage { get; set; }
}