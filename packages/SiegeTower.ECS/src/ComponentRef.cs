/*
	Exists for serialization

	eg:
	{
		guid EntityID;
	}
*/

public class ComponentRef<T> where T : Component
{
	public T? Value { get; set; }

	public bool HasValue => Value is not null;

	public ComponentRef(T? value = null)
	{
		Value = value;
	}

	public void Clear() => Value = null;

	public T? Get() => Value;

	public static implicit operator ComponentRef<T>(T value) => new(value);
}
