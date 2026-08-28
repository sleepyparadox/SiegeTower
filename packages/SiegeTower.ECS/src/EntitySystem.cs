public static class EntitySystem
{
	#region Single Components

	public static T AddNewEntityAndComponent<T>(this EntityStorage storage, Func<EntityStorage, Guid, T> initComponent) where T : Component
	{
		var e = Guid.NewGuid();
		storage.Entities.Add(e);
		var c = AddComponent(storage, e, initComponent);
		return c;
	}

	public static T AddComponent<T>(this EntityStorage storage, Guid e, Func<EntityStorage, Guid, T> initComponent) where T : Component
	{
		var c = initComponent(storage, e);
		ArgumentNullException.ThrowIfNull(c);
		storage.UpsertComponentDict<T>().Add(e, c);
		return c;
	}

	public static T AddComponent<T>(this Component baseComponent, Func<EntityStorage, Guid, T> initComponent) where T : Component
	{
		var storage = baseComponent.EntityStorage;
		var e = baseComponent.EntityID;
		var c = initComponent(storage, e);
		ArgumentNullException.ThrowIfNull(c);
		storage.UpsertComponentDict<T>().Add(e, c);
		return c;
	}

	public static IEnumerable<T> GetComponents<T>(this EntityStorage storage) where T : Component
	{
		if (storage.Components.TryGetValue(typeof(T), out var componentDict))
		{
			return componentDict.Values.Cast<T>();
		}
		return Enumerable.Empty<T>();
	}

	public static bool TryGetComponent<T>(this EntityStorage storage, Guid e, out T? c) where T : Component
	{
		if (storage.Components.TryGetValue(typeof(T), out var componentDict))
		{
			if (componentDict.TryGetValue(e, out var component))
			{
				c = (T)component;
				return true;
			}
		}
		c = null;
		return false;
	}

	public static bool TryGetComponent<T>(this Component baseComponent, Guid e, out T? c) where T : Component
	{
		var storage = baseComponent.EntityStorage;
		return storage.TryGetComponent<T>(e, out c);
	}

	public static void TryDeleteComponent<T>(this EntityStorage storage, T c) where T : Component
	{
		if (storage.Components.TryGetValue(typeof(T), out var componentDict))
		{
			if (componentDict.ContainsKey(c.EntityID))
			{
				componentDict.Remove(c.EntityID);
			}
		}

		if (c is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	public static void TryDeleteEntity(this EntityStorage storage, Guid e)
	{
		storage.Entities.Remove(e);
		foreach (var componentDict in storage.Components.Values)
		{
			if (componentDict.TryGetValue(e, out var c))
			{
				componentDict.Remove(e);

				if (c is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
	}

	#endregion

	#region Tuple Components

	public static Tuple<T1, T2> AddNewEntityAndComponents<T1, T2>(this EntityStorage storage, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2) where T1 : Component where T2 : Component
	{
		var e = Guid.NewGuid();
		storage.Entities.Add(e);
		var c1 = AddComponent(storage, e, init1);
		var c2 = AddComponent(storage, e, init2);
		return new Tuple<T1, T2>(c1, c2);
	}

	public static Tuple<T1, T2> AddComponents<T1, T2>(this EntityStorage storage, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2) where T1 : Component where T2 : Component
	{
		var c1 = init1(storage, e);
		var c2 = init2(storage, e);
		storage.UpsertComponentDict<T1>().Add(e, c1);
		storage.UpsertComponentDict<T2>().Add(e, c2);
		return new Tuple<T1, T2>(c1, c2);
	}

	public static Tuple<T1, T2, T3> AddComponents<T1, T2, T3>(this EntityStorage storage, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3) where T1 : Component where T2 : Component where T3 : Component
	{
		var c1 = init1(storage, e);
		var c2 = init2(storage, e);
		var c3 = init3(storage, e);
		storage.UpsertComponentDict<T1>().Add(e, c1);
		storage.UpsertComponentDict<T2>().Add(e, c2);
		storage.UpsertComponentDict<T3>().Add(e, c3);
		return new Tuple<T1, T2, T3>(c1, c2, c3);
	}

	public static Tuple<T1, T2, T3, T4> AddComponents<T1, T2, T3, T4>(this EntityStorage storage, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3, Func<EntityStorage, Guid, T4> init4) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
	{
		var c1 = init1(storage, e);
		var c2 = init2(storage, e);
		var c3 = init3(storage, e);
		var c4 = init4(storage, e);
		storage.UpsertComponentDict<T1>().Add(e, c1);
		storage.UpsertComponentDict<T2>().Add(e, c2);
		storage.UpsertComponentDict<T3>().Add(e, c3);
		storage.UpsertComponentDict<T4>().Add(e, c4);
		return new Tuple<T1, T2, T3, T4>(c1, c2, c3, c4);
	}

	public static Tuple<T1, T2, T3, T4, T5> AddComponents<T1, T2, T3, T4, T5>(this EntityStorage storage, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3, Func<EntityStorage, Guid, T4> init4, Func<EntityStorage, Guid, T5> init5) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
	{
		var c1 = init1(storage, e);
		var c2 = init2(storage, e);
		var c3 = init3(storage, e);
		var c4 = init4(storage, e);
		var c5 = init5(storage, e);
		storage.UpsertComponentDict<T1>().Add(e, c1);
		storage.UpsertComponentDict<T2>().Add(e, c2);
		storage.UpsertComponentDict<T3>().Add(e, c3);
		storage.UpsertComponentDict<T4>().Add(e, c4);
		storage.UpsertComponentDict<T5>().Add(e, c5);
		return new Tuple<T1, T2, T3, T4, T5>(c1, c2, c3, c4, c5);
	}

	public static Tuple<T1, T2, T3, T4, T5, T6> AddComponents<T1, T2, T3, T4, T5, T6>(this EntityStorage storage, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3, Func<EntityStorage, Guid, T4> init4, Func<EntityStorage, Guid, T5> init5, Func<EntityStorage, Guid, T6> init6) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
	{
		var c1 = init1(storage, e);
		var c2 = init2(storage, e);
		var c3 = init3(storage, e);
		var c4 = init4(storage, e);
		var c5 = init5(storage, e);
		var c6 = init6(storage, e);
		storage.UpsertComponentDict<T1>().Add(e, c1);
		storage.UpsertComponentDict<T2>().Add(e, c2);
		storage.UpsertComponentDict<T3>().Add(e, c3);
		storage.UpsertComponentDict<T4>().Add(e, c4);
		storage.UpsertComponentDict<T5>().Add(e, c5);
		storage.UpsertComponentDict<T6>().Add(e, c6);
		return new Tuple<T1, T2, T3, T4, T5, T6>(c1, c2, c3, c4, c5, c6);
	}

	public static Tuple<T1, T2> AddComponents<T1, T2>(this Component baseComponent, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2) where T1 : Component where T2 : Component
		=> AddComponents(baseComponent.EntityStorage, e, init1, init2);

	public static Tuple<T1, T2, T3> AddComponents<T1, T2, T3>(this Component baseComponent, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3) where T1 : Component where T2 : Component where T3 : Component
		=> AddComponents(baseComponent.EntityStorage, e, init1, init2, init3);

	public static Tuple<T1, T2, T3, T4> AddComponents<T1, T2, T3, T4>(this Component baseComponent, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3, Func<EntityStorage, Guid, T4> init4) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
		=> AddComponents(baseComponent.EntityStorage, e, init1, init2, init3, init4);

	public static Tuple<T1, T2, T3, T4, T5> AddComponents<T1, T2, T3, T4, T5>(this Component baseComponent, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3, Func<EntityStorage, Guid, T4> init4, Func<EntityStorage, Guid, T5> init5) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
		=> AddComponents(baseComponent.EntityStorage, e, init1, init2, init3, init4, init5);

	public static Tuple<T1, T2, T3, T4, T5, T6> AddComponents<T1, T2, T3, T4, T5, T6>(this Component baseComponent, Guid e, Func<EntityStorage, Guid, T1> init1, Func<EntityStorage, Guid, T2> init2, Func<EntityStorage, Guid, T3> init3, Func<EntityStorage, Guid, T4> init4, Func<EntityStorage, Guid, T5> init5, Func<EntityStorage, Guid, T6> init6) where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
		=> AddComponents(baseComponent.EntityStorage, e, init1, init2, init3, init4, init5, init6);

	#endregion

	#region WhereGetComponents

	public static IEnumerable<Tuple<TBase, T1>> WhereGetComponents<TBase, T1>(this EntityStorage storage) where TBase : Component where T1 : Component
		=> storage.GetComponents<TBase>().WhereGetComponents<TBase, T1>();

	public static IEnumerable<Tuple<TBase, T1, T2>> WhereGetComponents<TBase, T1, T2>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component
		=> storage.GetComponents<TBase>().WhereGetComponents<TBase, T1, T2>();

	public static IEnumerable<Tuple<TBase, T1, T2, T3>> WhereGetComponents<TBase, T1, T2, T3>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component
		=> storage.GetComponents<TBase>().WhereGetComponents<TBase, T1, T2, T3>();

	public static IEnumerable<Tuple<TBase, T1, T2, T3, T4>> WhereGetComponents<TBase, T1, T2, T3, T4>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component
		=> storage.GetComponents<TBase>().WhereGetComponents<TBase, T1, T2, T3, T4>();

	public static IEnumerable<Tuple<TBase, T1, T2, T3, T4, T5>> WhereGetComponents<TBase, T1, T2, T3, T4, T5>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
		=> storage.GetComponents<TBase>().WhereGetComponents<TBase, T1, T2, T3, T4, T5>();

	public static IEnumerable<Tuple<TBase, T1, T2, T3, T4, T5, T6>> WhereGetComponents<TBase, T1, T2, T3, T4, T5, T6>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
		=> storage.GetComponents<TBase>().WhereGetComponents<TBase, T1, T2, T3, T4, T5, T6>();

	public static IEnumerable<Tuple<TBase, T1>> WhereGetComponents<TBase, T1>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			if (baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1))
			{
				yield return new Tuple<TBase, T1>(baseComponent, c1!);
			}
		}
	}


	public static IEnumerable<Tuple<TBase, T1, T2>> WhereGetComponents<TBase, T1, T2>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			if (baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1)
				&& baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2))
			{
				yield return new Tuple<TBase, T1, T2>(baseComponent, c1!, c2!);
			}
		}
	}

	public static IEnumerable<Tuple<TBase, T1, T2, T3>> WhereGetComponents<TBase, T1, T2, T3>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			if (baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1)
				&& baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2)
				&& baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3))
			{
				yield return new Tuple<TBase, T1, T2, T3>(baseComponent, c1!, c2!, c3!);
			}
		}
	}

	public static IEnumerable<Tuple<TBase, T1, T2, T3, T4>> WhereGetComponents<TBase, T1, T2, T3, T4>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			if (baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1)
				&& baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2)
				&& baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3)
				&& baseComponent.EntityStorage.TryGetComponent<T4>(baseComponent.EntityID, out var c4))
			{
				yield return new Tuple<TBase, T1, T2, T3, T4>(baseComponent, c1!, c2!, c3!, c4!);
			}
		}
	}

	public static IEnumerable<Tuple<TBase, T1, T2, T3, T4, T5>> WhereGetComponents<TBase, T1, T2, T3, T4, T5>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			if (baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1)
				&& baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2)
				&& baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3)
				&& baseComponent.EntityStorage.TryGetComponent<T4>(baseComponent.EntityID, out var c4)
				&& baseComponent.EntityStorage.TryGetComponent<T5>(baseComponent.EntityID, out var c5))
			{
				yield return new Tuple<TBase, T1, T2, T3, T4, T5>(baseComponent, c1!, c2!, c3!, c4!, c5!);
			}
		}
	}

	public static IEnumerable<Tuple<TBase, T1, T2, T3, T4, T5, T6>> WhereGetComponents<TBase, T1, T2, T3, T4, T5, T6>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			if (baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1)
				&& baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2)
				&& baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3)
				&& baseComponent.EntityStorage.TryGetComponent<T4>(baseComponent.EntityID, out var c4)
				&& baseComponent.EntityStorage.TryGetComponent<T5>(baseComponent.EntityID, out var c5)
				&& baseComponent.EntityStorage.TryGetComponent<T6>(baseComponent.EntityID, out var c6))
			{
				yield return new Tuple<TBase, T1, T2, T3, T4, T5, T6>(baseComponent, c1!, c2!, c3!, c4!, c5!, c6!);
			}
		}
	}

	#endregion

	#region TryGetComponents

	public static IEnumerable<Tuple<TBase, T1?>> TryGetComponents<TBase, T1>(this EntityStorage storage) where TBase : Component where T1 : Component
		=> storage.GetComponents<TBase>().TryGetComponents<TBase, T1>();

	public static IEnumerable<Tuple<TBase, T1?, T2?>> TryGetComponents<TBase, T1, T2>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component
		=> storage.GetComponents<TBase>().TryGetComponents<TBase, T1, T2>();

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?>> TryGetComponents<TBase, T1, T2, T3>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component
		=> storage.GetComponents<TBase>().TryGetComponents<TBase, T1, T2, T3>();

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?, T4?>> TryGetComponents<TBase, T1, T2, T3, T4>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component
		=> storage.GetComponents<TBase>().TryGetComponents<TBase, T1, T2, T3, T4>();

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?, T4?, T5?>> TryGetComponents<TBase, T1, T2, T3, T4, T5>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
		=> storage.GetComponents<TBase>().TryGetComponents<TBase, T1, T2, T3, T4, T5>();

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?, T4?, T5?, T6?>> TryGetComponents<TBase, T1, T2, T3, T4, T5, T6>(this EntityStorage storage) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
		=> storage.GetComponents<TBase>().TryGetComponents<TBase, T1, T2, T3, T4, T5, T6>();

	public static IEnumerable<Tuple<TBase, T1?>> TryGetComponents<TBase, T1>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1);
			yield return new Tuple<TBase, T1?>(baseComponent, c1);
		}
	}

	public static IEnumerable<Tuple<TBase, T1?, T2?>> TryGetComponents<TBase, T1, T2>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1);
			baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2);
			yield return new Tuple<TBase, T1?, T2?>(baseComponent, c1, c2);
		}
	}

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?>> TryGetComponents<TBase, T1, T2, T3>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1);
			baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2);
			baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3);
			yield return new Tuple<TBase, T1?, T2?, T3?>(baseComponent, c1, c2, c3);
		}
	}

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?, T4?>> TryGetComponents<TBase, T1, T2, T3, T4>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1);
			baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2);
			baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3);
			baseComponent.EntityStorage.TryGetComponent<T4>(baseComponent.EntityID, out var c4);
			yield return new Tuple<TBase, T1?, T2?, T3?, T4?>(baseComponent, c1, c2, c3, c4);
		}
	}

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?, T4?, T5?>> TryGetComponents<TBase, T1, T2, T3, T4, T5>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1);
			baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2);
			baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3);
			baseComponent.EntityStorage.TryGetComponent<T4>(baseComponent.EntityID, out var c4);
			baseComponent.EntityStorage.TryGetComponent<T5>(baseComponent.EntityID, out var c5);
			yield return new Tuple<TBase, T1?, T2?, T3?, T4?, T5?>(baseComponent, c1, c2, c3, c4, c5);
		}
	}

	public static IEnumerable<Tuple<TBase, T1?, T2?, T3?, T4?, T5?, T6?>> TryGetComponents<TBase, T1, T2, T3, T4, T5, T6>(this IEnumerable<TBase> baseComponents) where TBase : Component where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component where T6 : Component
	{
		foreach (var baseComponent in baseComponents)
		{
			baseComponent.EntityStorage.TryGetComponent<T1>(baseComponent.EntityID, out var c1);
			baseComponent.EntityStorage.TryGetComponent<T2>(baseComponent.EntityID, out var c2);
			baseComponent.EntityStorage.TryGetComponent<T3>(baseComponent.EntityID, out var c3);
			baseComponent.EntityStorage.TryGetComponent<T4>(baseComponent.EntityID, out var c4);
			baseComponent.EntityStorage.TryGetComponent<T5>(baseComponent.EntityID, out var c5);
			baseComponent.EntityStorage.TryGetComponent<T6>(baseComponent.EntityID, out var c6);
			yield return new Tuple<TBase, T1?, T2?, T3?, T4?, T5?, T6?>(baseComponent, c1, c2, c3, c4, c5, c6);
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