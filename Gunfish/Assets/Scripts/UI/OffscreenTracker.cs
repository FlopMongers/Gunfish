using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class OffscreenTracker : MonoBehaviour
{
	public GameObject goToTrack;
    private new CanvasRenderer renderer;

    private float minSize = 0.8f;
    private float maxSize = 2.5f;

    void Start() {
        renderer = GetComponent<CanvasRenderer>();
    }

	void Update () {
		Vector3 v3Screen = Camera.main.WorldToViewportPoint(goToTrack.transform.position);
		if (v3Screen.x > -0.01f && v3Screen.x < 1.01f && v3Screen.y > -0.01f && v3Screen.y < 1.01f)
			renderer.cull = true;
		else
		{
			renderer.cull = false;
			v3Screen.x = Mathf.Clamp (v3Screen.x, 0.01f, 0.99f);
			v3Screen.y = Mathf.Clamp (v3Screen.y, 0.01f, 0.99f);
			transform.position = Camera.main.ViewportToWorldPoint (v3Screen);
            transform.rotation = goToTrack.transform.rotation;
            var dist = (transform.position - goToTrack.transform.position).magnitude;
            var size = (maxSize-minSize)*(1/(1 + dist)) + minSize;
            transform.localScale = Vector3.one * size;
		}
	}
}
