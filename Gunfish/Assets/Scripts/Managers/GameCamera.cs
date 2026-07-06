using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameCamera : Singleton<GameCamera> {
    public CinemachineTargetGroup targetGroup;
    public CinemachineConfiner2D confiner;
    public CinemachineFramingTransposer composer;

    // Confiner2D only ever repositions the camera -- it can't stop FramingTransposer's
    // auto-zoom from choosing an orthographic size large enough to render past the
    // bounds even while the camera itself stays inside them. Deriving the zoom cap
    // from the bounds polygon (instead of a hand-tuned per-level number) keeps it
    // from drifting out of sync as level geometry changes.
    const float maxZoomOutSafetyMargin = 0.9f;

    // Each live fish gets a small proxy transform in the target group instead of being
    // tracked directly. The proxy follows the fish's position while it's inside the
    // camera bounds, and clamps to the nearest point on the bounds edge once the fish
    // exits -- so a fish drifting toward a kill plane eases the framing toward the
    // edge continuously instead of snapping in/out of the group.
    readonly Dictionary<Transform, Transform> fishProxies = new Dictionary<Transform, Transform>();

#if UNITY_EDITOR
    // CinemachineFramingTransposer lives on Cinemachine's hidden "cm" pipeline child,
    // which doesn't appear in the Hierarchy or the object picker. Wire it up in
    // code instead: right-click the GameCamera component header and run this.
    [ContextMenu("Auto-Wire Confiner/Composer Refs")]
    void AutoWireCameraRefs() {
        confiner = GetComponentInChildren<CinemachineConfiner2D>();
        var vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        composer = vcam == null ? null : vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"GameCamera.AutoWireCameraRefs: confiner={confiner}, composer={composer}");
    }
#endif

    public void Start() {
        targetGroup.AddMember(transform, 1, 1);
        ClampZoomToBounds();
    }

    void Update() {
        if (confiner == null || confiner.m_BoundingShape2D == null)
            return;
        foreach (var kvp in fishProxies) {
            Vector2 clamped = confiner.m_BoundingShape2D.ClosestPoint(kvp.Key.position);
            kvp.Value.position = new Vector3(clamped.x, clamped.y, kvp.Key.position.z);
        }
    }

    void ClampZoomToBounds() {
        if (confiner == null || composer == null || confiner.m_BoundingShape2D == null || Camera.main == null) {
            Debug.LogWarning($"GameCamera.ClampZoomToBounds bailed out: confiner={confiner}, composer={composer}, " +
                $"boundingShape={(confiner == null ? "n/a" : confiner.m_BoundingShape2D?.ToString() ?? "null")}, " +
                $"Camera.main={Camera.main}");
            return;
        }

        Bounds bounds = confiner.m_BoundingShape2D.bounds;
        float aspect = Camera.main.aspect;
        float maxOrthoSize = Mathf.Max(bounds.size.x / aspect, bounds.size.y) / 2f;
        composer.m_MaximumOrthoSize = maxOrthoSize * maxZoomOutSafetyMargin;
        Debug.Log($"GameCamera.ClampZoomToBounds: boundsSize={bounds.size}, aspect={aspect}, " +
            $"computedMaxOrthoSize={maxOrthoSize}, appliedMaximumOrthoSize={composer.m_MaximumOrthoSize}");
    }

    // Keeps the camera anchored at the map center only while no fish are present;
    // once a fish registers, the anchor is dropped so it stops biasing framing/zoom.
    public void AddFishMember(Transform t) {
        if (fishProxies.Count == 0)
            targetGroup.RemoveMember(transform);

        var proxy = new GameObject($"CameraTargetProxy_{t.name}").transform;
        proxy.SetParent(transform, worldPositionStays: true);
        proxy.position = t.position;
        fishProxies.Add(t, proxy);
        targetGroup.AddMember(proxy, 1, 1);
    }

    public void RemoveFishMember(Transform t) {
        if (fishProxies.TryGetValue(t, out var proxy)) {
            targetGroup.RemoveMember(proxy);
            fishProxies.Remove(t);
            Destroy(proxy.gameObject);
        }
        if (fishProxies.Count == 0)
            targetGroup.AddMember(transform, 1, 1);
    }
}
