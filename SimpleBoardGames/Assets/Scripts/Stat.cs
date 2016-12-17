using System;


/// <summary>
/// A value with an owner.
/// An even is raised whenever the value changes.
/// </summary>
public class Stat<T, OwnerType>
{
	public T Value
	{
		get { return val; }
		set
		{
			T oldVal = val;
			val = value;

			if (OnChanged != null)
				OnChanged(Owner, oldVal, val);
		}
	}
	private T val;

	/// <summary>
	/// Raised when this stat changes.
	/// The first argument is the owner of this stat.
	/// The second and third arguments are the old and new value, respectively.
	/// </summary>
	public event Action<OwnerType, T, T> OnChanged;

	public OwnerType Owner { get; private set; }


	public Stat(OwnerType owner, T initialValue)
	{
		Owner = owner;
		val = initialValue;
	}


	//You can implicitly cast this class to the data it contains.
	public static implicit operator T(Stat<T, OwnerType> s)
	{
		return s.val;
	}
}