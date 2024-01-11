using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Death Match UI Player Color Image will reflect the hue of its parent Player Widget.
/// </summary>
public class DeathMatchUIPlayerColorImage : MonoBehaviour {
    private DeathMatchUIPlayerWidget playerWidget;
    private Image image;
    private Color baseColor;

    // Start is called before the first frame update
    void Awake() {
        image = GetComponent<Image>();
        baseColor = image.color;
        playerWidget = GetComponentInParent<DeathMatchUIPlayerWidget>();
        playerWidget.OnPlayerColorChange.AddListener(SetColor);
    }

    private void SetColor(Color color) {
        image.color = color * baseColor;
    }
}