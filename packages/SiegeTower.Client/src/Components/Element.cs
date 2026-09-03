public class Element : Component
{
	public string Id { get; set; }
	public ElementColor Color { get; set; }
	public ElementState State { get; set; }
	public GridAlignment Alignment { get; set; }
	public int? GridIndent { get; set; }

	public ComponentRef<Element> Parent { get; set; }

	public ComponentRefList<Element> Children { get; set; }

	public Element(Entity entity, string id)
		: base(entity)
	{
		Id = id;
		Color = ElementColor.None;
		State = ElementState.None;
		Alignment = GridAlignment.Default;
		Parent = new ComponentRef<Element>();
		Children = new ComponentRefList<Element>();
	}

	public int Index => Parent.HasValue
		? Parent.Get()!.Children.IndexOf(this)
		: -1;
}