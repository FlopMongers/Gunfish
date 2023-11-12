using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    public static T Instance { get; protected set; }

    public static bool InstanceExists {
        get { return Instance != null; }
    }

    public bool Initialized { get; protected set; }
    public bool Destroyed { get; protected set; }

    protected virtual void Awake() {
        if (InstanceExists) {
            Destroy(gameObject);
            Destroyed = true;
        }
        else {
            Instance = (T)this;
        }
    }

    protected virtual void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public virtual void Initialize() {
        if (!InstanceExists) {
            throw new UnityException($"{typeof(T).Name} is a singleton but has not been Instantiated. Has a GameObject with this component been added to the scene?");
        }
        Initialized = true;
    }
}