using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}
