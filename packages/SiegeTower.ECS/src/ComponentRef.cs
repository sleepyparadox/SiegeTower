public struct ComponentRef<T> where T : Component
{
	public EntityStorage EntityStorage { get; set; }

	public Guid? RefID { get; set; }

	public ComponentRef(EntityStorage entityStorage)
	{
		EntityStorage = entityStorage;
	}

	public static implicit operator ComponentRef<T>(T? component)
	{
		if (component is null)
		{
			return default;
		}

		return new ComponentRef<T>(component.EntityStorage)
		{
			RefID = component.EntityID
		};
	}

	public T Get()
	{
		if (TryGet(out var component) && component is not null)
		{
			return component;
		}

		throw new InvalidOperationException($"Component reference {typeof(T).Name}:{RefID} could not be resolved.");
	}

	public bool TryGet(out T? component)
	{
		if (RefID.HasValue && EntityStorage is not null)
		{
			return EntityStorage.TryGetComponent(RefID.Value, out component);
		}

		component = null;
		return false;
	}

	public bool HasValue => RefID.HasValue;
}