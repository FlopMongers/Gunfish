using System.Collections;
using UnityEngine;

public class DestroyAfterFrames : MonoBehaviour
{
    [SerializeField] private int destroyAfterFrames = 0;
    void Awake() {
        if (destroyAfterFrames > 0) {
            StartCoroutine(DestroyAfterFramesElapsed());
        } else {
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterFramesElapsed() {
        for (int i = 0; i < destroyAfterFrames; i++) {
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}
