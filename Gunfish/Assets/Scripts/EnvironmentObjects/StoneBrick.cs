using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(StoneBrickEditor))]
public class StoneBrickEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var scripts = targets.OfType<StoneBrick>();
        if (GUILayout.Button("GARBULATE")) {
            foreach (var script in scripts) {
                script.Garbulate();
            }
        }
    }
}
#endif

public class StoneBrick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Garbulate() {
        GetComponent<SpriteRenderer>().material.mainTextureOffset = Random.insideUnitCircle;
    }
}
