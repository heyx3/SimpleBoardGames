using UnityEngine;


/// <summary>
/// Like the Singleton class, except the GameObject owning it has DontDestroyOnLoad() called,
///     and duplicate instances destroy their GameObjects to ensure there is always just one.
/// </summary>
public class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
{
	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		else
		{
			Instance = (T)this;
			DontDestroyOnLoad(gameObject);
		}
	}
	protected virtual void OnDestroy()
	{
		UnityEngine.Assertions.Assert.AreEqual(this, Instance, "This should never happen!");
		Instance = null;
	}
}