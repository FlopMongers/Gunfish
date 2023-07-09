/// <summary>
/// Singleton that persists across multiple scenes
/// </summary>
public class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
{
	public bool persistRoot;

	protected override void Awake()
	{
		base.Awake();
		if (persistRoot)
			DontDestroyOnLoad(transform.root.gameObject);
		else
			DontDestroyOnLoad(gameObject);
	}
}