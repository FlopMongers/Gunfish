using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotatingImage : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    [Range(0.1f, 5f)]
    public float interval = 0.5f;

    private int index;
    // Start is called before the first frame update
    private void Start()
    {
        index = 0;
        InvokeRepeating("CycleSprite", 0.0f, interval);
    }

    private void CycleSprite()
    {
        GetComponent<Image>().sprite = sprites[index];
        index = (index + 1) % sprites.Count;
    }
}
