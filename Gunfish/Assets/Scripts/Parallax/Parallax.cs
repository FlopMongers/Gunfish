using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Parallax : MonoBehaviour {
    private ParallaxSettings settings;
    private SpriteRenderer spriteRenderer;
    private Material material;

    private void Start() {
        settings = Resources.Load<ParallaxSettings>("Settings/ParallaxSettings");
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = new Material(spriteRenderer.material);
        spriteRenderer.material = material;
    }

    private void Update() {
        material.mainTextureOffset = new Vector2(Time.time, 0f);
    }
}
