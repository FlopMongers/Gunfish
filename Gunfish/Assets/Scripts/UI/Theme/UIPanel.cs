using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class UIPanel : MonoBehaviour {
    [SerializeField] private Color fillColor = Color.white;
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private float cornerRadius = 16f;
    [SerializeField] private float borderWidth = 0f;
    [SerializeField] private float edgeSoftness = 1.5f;

    private static Shader roundedRectShader;

    private Image image;
    private RectTransform rectTransform;
    private Material materialInstance;

    public Color FillColor { get => fillColor; set { fillColor = value; Apply(); } }
    public Color BorderColor { get => borderColor; set { borderColor = value; Apply(); } }
    public float CornerRadius { get => cornerRadius; set { cornerRadius = Mathf.Max(0f, value); Apply(); } }
    public float BorderWidth { get => borderWidth; set { borderWidth = Mathf.Max(0f, value); Apply(); } }

    private void OnEnable() {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        image.sprite = null;
        image.type = Image.Type.Simple;

        if (roundedRectShader == null) {
            roundedRectShader = Shader.Find("Gunfish/UI/RoundedRect");
        }
        materialInstance = new Material(roundedRectShader);
        image.material = materialInstance;

        transform.hasChanged = false;
        Apply();
    }

    private void OnDisable() {
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

    private void OnRectTransformDimensionsChange() {
        Apply();
    }

    private void OnValidate() {
        cornerRadius = Mathf.Max(0f, cornerRadius);
        borderWidth = Mathf.Max(0f, borderWidth);
        edgeSoftness = Mathf.Max(0f, edgeSoftness);
        Apply();
    }

    private void Apply() {
        if (materialInstance == null || rectTransform == null) return;

        image.color = fillColor;
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
