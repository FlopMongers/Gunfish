using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    public static T instance { get; protected set; }

    public static bool InstanceExists {
        get { return instance != null; }
    }

    protected bool destroyed;

    protected virtual void Awake() {
        if (InstanceExists) {
            Destroy(gameObject);
            destroyed = true;
        }
        else {
            instance = (T)this;
        }
    }

    protected virtual void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }
}