public class Element : Component
{
	public string Id { get; set; }

	public ComponentRef<Element> Parent { get; set; }

	public ComponentRefList<Element> Children { get; set; }

	public Element(EntityStorage entityStorage, Guid entityID, string id)
		: base(entityStorage, entityID)
	{
		Id = id;
		Parent = new ComponentRef<Element>(entityStorage);
		Children = new ComponentRefList<Element>(entityStorage);
	}

	public int Index => Parent.RefID.HasValue
		? Parent.Get()!.Children.IndexOf(this)
		: -1;
}