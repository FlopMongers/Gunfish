using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(UIPanel))]
public class UIButton : MonoBehaviour {
    [SerializeField] private TMP_Text label;

    private void Awake() {
        var button = GetComponent<Button>();
        var image = GetComponent<Image>();

        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = image;

        if (label != null && UITheme.ButtonLabel.Font != null) {
            label.font = UITheme.ButtonLabel.Font;
            label.fontSize = UITheme.ButtonLabel.Size;
            label.fontStyle = UITheme.ButtonLabel.Style;
        }
    }
}
