public static class EntitySystem
{
	#region Entity

	public static Entity NewEntity(this EntityStorage storage)
		=> NewEntity(storage, out _);

	public static Entity NewEntity(this EntityStorage storage, out Entity result)
	{
		result = new Entity(storage, Guid.NewGuid());
		storage.Entities.Add(result.ID);
		return result;
	}

	#endregion


	#region AddComponent

	public static T AddComponent<T>(this Entity entity) where T : Component
		=> AddComponent<T>(entity, e => (T)Activator.CreateInstance(typeof(T), e)!, out _);

	public static T AddComponent<T>(this Entity entity, out T result) where T : Component
		=> AddComponent<T>(entity, e => (T)Activator.CreateInstance(typeof(T), e)!, out result);

	public static T AddComponent<T>(this Entity entity, Func<Entity, T> constructor) where T : Component
		=> AddComponent<T>(entity, constructor, out _);

	public static T AddComponent<T>(this Component baseComponent) where T : Component
		=> AddComponent<T>(baseComponent.Entity, e => (T)Activator.CreateInstance(typeof(T), e)!, out _);

	public static T AddComponent<T>(this Component baseComponent, out T result) where T : Component
		=> AddComponent<T>(baseComponent.Entity, e => (T)Activator.CreateInstance(typeof(T), e)!, out result);

	public static T AddComponent<T>(this Component baseComponent, Func<Entity, T> constructor) where T : Component
		=> AddComponent<T>(baseComponent.Entity, constructor, out _);

	public static T AddComponent<T>(this Entity entity, Func<Entity, T> constructor, out T result) where T : Component
	{
		var firstMissingRequirement = entity.GetMissingRequirements<T>().FirstOrDefault();
		if(firstMissingRequirement != null)
		{
			throw new InvalidOperationException($"Cannot add component of type {typeof(T).Name} to entity {entity.ID} because {typeof(T).Name} requires a {firstMissingRequirement.Name} are not met.");
		}

		result = constructor(entity)!;
		entity.EntityStorage.UpsertComponentDict<T>().Add(entity.ID, result);
		return result!;
	}

	static IEnumerable<Type> GetMissingRequirements<T>(this Entity entity) where T : Component
	{
		var requirements = typeof(T).GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequires<>)).ToList();
		foreach (var requirement in requirements)
		{
			var requiredType = requirement.GetGenericArguments()[0];
			if (!entity.EntityStorage.Components.TryGetValue(requiredType, out var componentDict) || !componentDict.ContainsKey(entity.ID))
			{
				yield return requiredType;
			}
		}
	}

	#endregion


	#region TryGet

	public static bool TryGetComponent<T>(this Entity entity, out T? c) where T : Component
	{
		if (entity.EntityStorage.Components.TryGetValue(typeof(T), out var componentDict))
		{
			if (componentDict.TryGetValue(entity.ID, out var component))
			{
				c = (T)component;
				return true;
			}
		}
		c = null;
		return false;
	}

	public static bool TryGetComponent<T>(this Component baseComponent, out T? c) where T : Component
	{
		return baseComponent.Entity.TryGetComponent(out c);
	}

	#region GetComponent

	public static T GetComponent<T>(this IRequires<T> baseComponent) where T : Component
		=> GetComponent<T>(baseComponent, out _);

	public static T GetComponent<T>(this IRequires<T> baseComponent, out T? c) where T : Component
	{
		baseComponent.Entity.TryGetComponent(out c);
		// Validation ensures component will exist
		return c!;
	}

	#endregion

	#endregion

	#region TryOutComponent

	public static TBase OutComponent<TBase, T>(this TBase baseComponent, out T? c) where T : Component where TBase : Component
	{
		baseComponent.Entity.TryGetComponent(out c);
		return baseComponent;
	}

	#endregion

	#region SelectComponents

	public static IEnumerable<T> SelectComponents<T>(this EntityStorage storage) where T : Component
	{
		return storage.Components.TryGetValue(typeof(T), out var componentDict)
			? componentDict.Values.Cast<T>()
			: Enumerable.Empty<T>();	
	}

	public static Tuple<T1, T2> SelectComponents<T1, T2>(this Entity entity) where T1 : Component where T2 : Component
	{
		entity.TryGetComponent(out T1? c1);
		entity.TryGetComponent(out T2? c2);
		return new Tuple<T1, T2>(c1!, c2!);
	}

	public static Tuple<T1, T2, T3> SelectComponents<T1, T2, T3>(this Entity entity) where T1 : Component where T2 : Component where T3 : Component
	{
		entity.TryGetComponent(out T1? c1);
		entity.TryGetComponent(out T2? c2);
		entity.TryGetComponent(out T3? c3);
		return new Tuple<T1, T2, T3>(c1!, c2!, c3!);
	}

	public static Tuple<T1, T2, T3, T4> SelectComponents<T1, T2, T3, T4>(this Entity entity) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
	{
		entity.TryGetComponent(out T1? c1);
		entity.TryGetComponent(out T2? c2);
		entity.TryGetComponent(out T3? c3);
		entity.TryGetComponent(out T4? c4);
		return new Tuple<T1, T2, T3, T4>(c1!, c2!, c3!, c4!);
	}

	public static Tuple<T1, T2, T3, T4, T5> SelectComponents<T1, T2, T3, T4, T5>(this Entity entity) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
	{
		entity.TryGetComponent(out T1? c1);
		entity.TryGetComponent(out T2? c2);
		entity.TryGetComponent(out T3? c3);
		entity.TryGetComponent(out T4? c4);
		entity.TryGetComponent(out T5? c5);
		return new Tuple<T1, T2, T3, T4, T5>(c1!, c2!, c3!, c4!, c5!);
	}

	public static Tuple<T1, T2, T3, T4, T5, T6> SelectComponents<T1, T2, T3, T4, T5, T6>(this Entity entity) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
	{
		entity.TryGetComponent(out T1? c1);
		entity.TryGetComponent(out T2? c2);
		entity.TryGetComponent(out T3? c3);
		entity.TryGetComponent(out T4? c4);
		entity.TryGetComponent(out T5? c5);
		entity.TryGetComponent(out T6? c6);
		return new Tuple<T1, T2, T3, T4, T5, T6>(c1!, c2!, c3!, c4!, c5!, c6!);
	}

	public static Tuple<T1, T2, T3, T4, T5, T6, T7> SelectComponents<T1, T2, T3, T4, T5, T6, T7>(this Entity entity) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component where T7 : Component
	{
		entity.TryGetComponent(out T1? c1);
		entity.TryGetComponent(out T2? c2);
		entity.TryGetComponent(out T3? c3);
		entity.TryGetComponent(out T4? c4);
		entity.TryGetComponent(out T5? c5);
		entity.TryGetComponent(out T6? c6);
		entity.TryGetComponent(out T7? c7);
		return new Tuple<T1, T2, T3, T4, T5, T6, T7>(c1!, c2!, c3!, c4!, c5!, c6!, c7!);
	}

	public static IEnumerable<Tuple<T1, T2>> SelectComponentsWhereExist<T1, T2>(this EntityStorage storage) where T1 : Component where T2 : Component
	=> storage.SelectComponents<T1>().SelectComponentsWhereExist<T1, T2>();

	public static IEnumerable<Tuple<T1, T2>> SelectComponentsWhereExist<T1, T2>(this IEnumerable<T1> initCollection) where T1 : Component where T2 : Component
	{
		foreach (var c1 in initCollection)
		{
			if (c1.Entity.TryGetComponent(out T2? c2))
			{
				yield return new Tuple<T1, T2>(c1, c2!);
			}
		}
	}

	public static IEnumerable<Tuple<T1, T2, T3>> SelectComponentsWhereExist<T1, T2, T3>(this EntityStorage storage) where T1 : Component where T2 : Component where T3 : Component
	=> storage.SelectComponents<T1>().SelectComponentsWhereExist<T1, T2, T3>();

	public static IEnumerable<Tuple<T1, T2, T3>> SelectComponentsWhereExist<T1, T2, T3>(this IEnumerable<T1> initCollection) where T1 : Component where T2 : Component where T3 : Component
	{
		foreach (var c1 in initCollection)
		{
			if (c1.Entity.TryGetComponent(out T2? c2) && c1.Entity.TryGetComponent(out T3? c3))
			{
				yield return new Tuple<T1, T2, T3>(c1, c2!, c3!);
			}
		}
	}

	public static IEnumerable<Tuple<T1, T2, T3, T4>> SelectComponentsWhereExist<T1, T2, T3, T4>(this EntityStorage storage) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
	=> storage.SelectComponents<T1>().SelectComponentsWhereExist<T1, T2, T3, T4>();

	public static IEnumerable<Tuple<T1, T2, T3, T4>> SelectComponentsWhereExist<T1, T2, T3, T4>(this IEnumerable<T1> initCollection) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
	{
		foreach (var c1 in initCollection)
		{
			if (c1.Entity.TryGetComponent(out T2? c2) && c1.Entity.TryGetComponent(out T3? c3) && c1.Entity.TryGetComponent(out T4? c4))
			{
				yield return new Tuple<T1, T2, T3, T4>(c1, c2!, c3!, c4!);
			}
		}
	}

	public static IEnumerable<Tuple<T1, T2, T3, T4, T5>> SelectComponentsWhereExist<T1, T2, T3, T4, T5>(this EntityStorage storage) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
	=> storage.SelectComponents<T1>().SelectComponentsWhereExist<T1, T2, T3, T4, T5>();

	public static IEnumerable<Tuple<T1, T2, T3, T4, T5>> SelectComponentsWhereExist<T1, T2, T3, T4, T5>(this IEnumerable<T1> initCollection) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
	{
		foreach (var c1 in initCollection)
		{
			if (c1.Entity.TryGetComponent(out T2? c2) && c1.Entity.TryGetComponent(out T3? c3) && c1.Entity.TryGetComponent(out T4? c4) && c1.Entity.TryGetComponent(out T5? c5))
			{
				yield return new Tuple<T1, T2, T3, T4, T5>(c1, c2!, c3!, c4!, c5!);
			}
		}
	}

	public static IEnumerable<Tuple<T1, T2, T3, T4, T5, T6>> SelectComponentsWhereExist<T1, T2, T3, T4, T5, T6>(this EntityStorage storage) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
	=> storage.SelectComponents<T1>().SelectComponentsWhereExist<T1, T2, T3, T4, T5, T6>();

	public static IEnumerable<Tuple<T1, T2, T3, T4, T5, T6>> SelectComponentsWhereExist<T1, T2, T3, T4, T5, T6>(this IEnumerable<T1> initCollection) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
	{
		foreach (var c1 in initCollection)
		{
			if (c1.Entity.TryGetComponent(out T2? c2) && c1.Entity.TryGetComponent(out T3? c3) && c1.Entity.TryGetComponent(out T4? c4) && c1.Entity.TryGetComponent(out T5? c5) && c1.Entity.TryGetComponent(out T6? c6))
			{
				yield return new Tuple<T1, T2, T3, T4, T5, T6>(c1, c2!, c3!, c4!, c5!, c6!);
			}
		}
	}

	public static IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7>> SelectComponentsWhereExist<T1, T2, T3, T4, T5, T6, T7>(this EntityStorage storage) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component where T7 : Component
	=> storage.SelectComponents<T1>().SelectComponentsWhereExist<T1, T2, T3, T4, T5, T6, T7>();

	public static IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7>> SelectComponentsWhereExist<T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<T1> initCollection) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component where T7 : Component
	{
		foreach (var c1 in initCollection)
		{
			if (c1.Entity.TryGetComponent(out T2? c2) && c1.Entity.TryGetComponent(out T3? c3) && c1.Entity.TryGetComponent(out T4? c4) && c1.Entity.TryGetComponent(out T5? c5) && c1.Entity.TryGetComponent(out T6? c6) && c1.Entity.TryGetComponent(out T7? c7))
			{
				yield return new Tuple<T1, T2, T3, T4, T5, T6, T7>(c1, c2!, c3!, c4!, c5!, c6!, c7!);
			}
		}
	}

	#endregion

	#region TryDelete

	public static void TryDeleteComponent<T>(this EntityStorage storage, T c) where T : Component
	{
		if (storage.Components.TryGetValue(typeof(T), out var componentDict))
		{
			if (componentDict.ContainsKey(c.Entity.ID))
			{
				componentDict.Remove(c.Entity.ID);
			}
		}

		if (c is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	public static void TryDeleteEntity(this Entity entity)
	{
		entity.EntityStorage.Entities.Remove(entity.ID);
		foreach (var componentDict in entity.EntityStorage.Components.Values)
		{
			if (componentDict.TryGetValue(entity.ID, out var c))
			{
				componentDict.Remove(entity.ID);

				if (c is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
	}

	#endregion

	static Dictionary<Guid, Component> UpsertComponentDict<T>(this EntityStorage storage) where T : Component
	{
		if (!storage.Components.TryGetValue(typeof(T), out var componentDict))
		{
			componentDict = new Dictionary<Guid, Component>();
			storage.Components.Add(typeof(T), componentDict);
		}
		return componentDict;

	}
}