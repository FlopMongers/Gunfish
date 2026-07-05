using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIPanel))]
public class UIButton : Button {
    [SerializeField] private TMP_Text label;

    protected override void Awake() {
        base.Awake();

        transition = Selectable.Transition.ColorTint;
        targetGraphic = GetComponent<UIPanel>();

        if (label != null && UITheme.ButtonLabel.Font != null) {
            label.font = UITheme.ButtonLabel.Font;
            label.fontSize = UITheme.ButtonLabel.Size;
            label.fontStyle = UITheme.ButtonLabel.Style;
        }
    }
}
