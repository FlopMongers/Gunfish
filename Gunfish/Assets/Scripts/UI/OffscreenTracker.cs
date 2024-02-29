using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class OffscreenTracker : MonoBehaviour
{
	public GameObject goToTrack;
    private CanvasRenderer canvasRenderer;

    private float minSize = 1.5f;
    private float maxSize = 2.8f;
	private float padding = 0.015f;
	private float inversePadding;
	public Color color;

    void Start() {
        canvasRenderer = GetComponent<CanvasRenderer>();
		inversePadding = 1 - padding;
		GetComponent<Image>().color = color;
    }

	void Update () {
        if (goToTrack == null || goToTrack.transform == null) {
            Destroy(gameObject);
            return;
        }
		Vector3 v3Screen = Camera.main.WorldToViewportPoint(goToTrack.transform.position);
		if (v3Screen.x > padding && v3Screen.x < inversePadding && v3Screen.y > padding && v3Screen.y < inversePadding)
			canvasRenderer.cull = true;
		else
		{
			canvasRenderer.cull = false;
			v3Screen.x = Mathf.Clamp (v3Screen.x, padding, inversePadding);
			v3Screen.y = Mathf.Clamp (v3Screen.y, padding, inversePadding);
			transform.position = Camera.main.ViewportToWorldPoint (v3Screen);
            transform.rotation = goToTrack.transform.rotation;
            var dist = (transform.position - goToTrack.transform.position).magnitude;
            var size = (maxSize-minSize)*(1/(1 + dist)) + minSize;
            transform.localScale = Vector3.one * size;
		}
	}
}
