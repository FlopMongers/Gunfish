using UnityEngine;

public class DestroyOnAwake : MonoBehaviour
{
    void Awake() {
        Destroy(gameObject);
    }
}
