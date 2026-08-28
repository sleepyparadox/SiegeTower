public struct ComponentRefList<T> where T : Component
{
	public EntityStorage EntityStorage { get; set; }

	public List<Guid> RefIDs { get; set; }

	public ComponentRefList(EntityStorage entityStorage)
	{
		EntityStorage = entityStorage;
		RefIDs = new List<Guid>();
	}

	public void Add(Guid e) => RefIDs.Add(e);
	public void Add(T c) => RefIDs.Add(c.EntityID);
	public bool Contains(Guid e) => RefIDs.Contains(e);
	public bool Contains(T c) => RefIDs.Contains(c.EntityID);
	public int IndexOf(Guid e) => RefIDs.IndexOf(e);
	public int IndexOf(T c) => RefIDs.IndexOf(c.EntityID);
	public void SetIndex(Guid e, int index) => RefIDs[index] = e;
	public void SetIndex(T c, int index) => RefIDs[index] = c.EntityID;
	public void Remove(Guid e) => RefIDs.Remove(e);
	public void Remove(T c) => RefIDs.Remove(c.EntityID);

	public IEnumerable<T> Get()
	{
		foreach (var refID in RefIDs)
		{
			if (EntityStorage.TryGetComponent(refID, out T? component) && component is not null)
			{
				yield return component;
			}
		}
	}
}