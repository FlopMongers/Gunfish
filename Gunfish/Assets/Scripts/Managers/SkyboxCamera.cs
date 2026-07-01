using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SkyboxCamera : Singleton<SkyboxCamera>
{
    private Camera mainCamera;
    private Camera _camera;
    private Vector2 refMainCameraPosition;
    private Vector3 basePosition;
    [SerializeField] private float trackingPanRatio;
    [SerializeField] private float trackingZoomRatio;
    private float baseFOV = 45;

    // Start is called before the first frame update
    void Start()
    {
        basePosition = transform.position;
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.position = (Vector3)((Vector2)mainCamera.transform.position - refMainCameraPosition) * trackingPanRatio + basePosition;
            GetComponent<Camera>().fieldOfView = mainCamera.orthographicSize * trackingZoomRatio + baseFOV;
        }
    }

    public void RegisterCamera(Camera camera)
    {
        mainCamera = camera;
        refMainCameraPosition = camera.transform.position;

        // URP renders this camera as a Base camera and the gameplay camera as an Overlay on top of it -
        // the old Built-in RP depth-ordering + "Don't Clear" trick between two independent cameras has
        // no URP equivalent, so the stack has to be wired at runtime since the gameplay camera lives in
        // a separately-loaded, prefab-instantiated scene.
        var baseCameraData = _camera.GetUniversalAdditionalCameraData();
        if (!baseCameraData.cameraStack.Contains(camera))
        {
            baseCameraData.cameraStack.Add(camera);
        }
    }
}
