using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIPanel : Image {
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private float cornerRadius = 16f;
    [SerializeField] private float borderWidth = 0f;
    [SerializeField] private float edgeSoftness = 1.5f;

    private static Shader roundedRectShader;
    private Material materialInstance;

    public Color BorderColor { get => borderColor; set { borderColor = value; Apply(); } }
    public float CornerRadius { get => cornerRadius; set { cornerRadius = Mathf.Max(0f, value); Apply(); } }
    public float BorderWidth { get => borderWidth; set { borderWidth = Mathf.Max(0f, value); Apply(); } }

    protected override void OnEnable() {
        base.OnEnable();
        sprite = null;
        type = Image.Type.Simple;

        if (roundedRectShader == null) {
            roundedRectShader = Shader.Find("Gunfish/UI/RoundedRect");
        }
        materialInstance = new Material(roundedRectShader);
        material = materialInstance;

        transform.hasChanged = false;
        Apply();
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (materialInstance != null) {
            DestroyImmediate(materialInstance);
            materialInstance = null;
        }
    }

    private void Update() {
        if (transform.hasChanged) {
            Apply();
            transform.hasChanged = false;
        }
    }

    protected override void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();
        Apply();
    }

#if UNITY_EDITOR
    protected override void OnValidate() {
        base.OnValidate();
        cornerRadius = Mathf.Max(0f, cornerRadius);
        borderWidth = Mathf.Max(0f, borderWidth);
        edgeSoftness = Mathf.Max(0f, edgeSoftness);
        Apply();
    }
#endif

    private void Apply() {
        if (materialInstance == null) return;

        Rect rect = rectTransform.rect;
        Vector3 scale = rectTransform.lossyScale;
        float width = rect.width * Mathf.Abs(scale.x);
        float height = rect.height * Mathf.Abs(scale.y);
        materialInstance.SetVector("_Size", new Vector4(width, height, 0, 0));
        materialInstance.SetFloat("_Radius", cornerRadius);
        materialInstance.SetFloat("_BorderWidth", borderWidth);
        materialInstance.SetFloat("_Softness", edgeSoftness);
        materialInstance.SetColor("_BorderColor", borderColor);
    }
}
