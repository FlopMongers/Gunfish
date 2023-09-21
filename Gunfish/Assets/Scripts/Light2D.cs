using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Light2D : MonoBehaviour {
    private Vector3 position;
    private float width;
    private float height;
    private Vector3 scale;

    private List<GameObject> objects;
    private List<Vector3> vertices;

    // Start is called before the first frame update
    void Start()
    {
                
    }

    private void LateUpdate() {
        UpdateLightArea();
        UpdateObjects();
        SortObjects();
    }

    private void UpdateObjects() {
        objects = new List<GameObject>(FindObjectsOfType<GameObject>());
    }

    private void SortObjects() {
        objects.Sort((GameObject a, GameObject b) => {
            var angleA = Mathf.Atan2(a.transform.position.y - transform.position.y, a.transform.position.x - transform.position.x);
            var angleB = Mathf.Atan2(b.transform.position.y - transform.position.y, b.transform.position.x - transform.position.x);
            if (angleA < angleB) {
                return -1;
            } else {
                return 1;
            }
        });
    }

    private void SortVertices() {

    }

    private void UpdateLightArea() {
        position = Camera.main.transform.position + Vector3.forward;
        height = Camera.main.orthographicSize * 2.0f;
        width = height * Camera.main.aspect;
        scale = new Vector3(width, height, 1f);

        transform.position = position;
        transform.localScale = scale;
    }

    private void OnDrawGizmos() {
        if (objects == null) {
            return;
        }


        for (int i = 0; i < objects.Count; i++) {


            float t = (float)i / (objects.Count - 1);
            Color c = Color.Lerp(Color.red, Color.green, t);

        }
    }
}
