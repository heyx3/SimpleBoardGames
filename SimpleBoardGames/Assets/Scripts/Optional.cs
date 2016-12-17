/// <summary>
/// Works like System.Nullable<>.
/// Can't actually use Nullable sometimes
///     because you don't know if a generic argument is a class or a struct.
/// Also Unity sometimes has a compiler error when using Nullable in a generic class.
/// </summary>
public struct Optional<T>
{
	public bool HasValue { get; private set; }
	public T Value
	{
		get { UnityEngine.Assertions.Assert.IsTrue(HasValue); return value; }
	}

	private T value;


	public Optional(T val) { HasValue = true;  value = val; }


	public static implicit operator T(Optional<T> o) { return o.Value; }
	public static implicit operator Optional<T>(T t) { return new Optional<T>(t); }
	
	//Little syntax hack to allow casting "null" to an Optional<T>.
	public static implicit operator Optional<T>(sbyte? c)
	{
		UnityEngine.Assertions.Assert.IsFalse(c.HasValue);
		return new Optional<T>();
	}
}