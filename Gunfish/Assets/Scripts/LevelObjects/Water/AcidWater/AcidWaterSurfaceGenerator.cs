using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(AcidWaterSurfaceGenerator))]
public class AcidWaterSurfaceGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var scripts = targets.OfType<AcidWaterSurfaceGenerator>();
        if (GUILayout.Button("GARBULATE")) {
            foreach (var script in scripts) {
                script.ClearCurrentNodes();
                script.Garbulate();
            }
        }
    }
}
#endif

public class AcidWaterSurfaceGenerator : WaterSurfaceGenerator
{
    public ParticleSystem acidBubbles;

    public override void Garbulate() {
        base.Garbulate();
        var shape = acidBubbles.shape;
        shape.scale = new Vector3(length, shape.scale.y, shape.scale.z);
        acidBubbles.transform.position = transform.position + (Vector3.up * height / 2);
    }
}
