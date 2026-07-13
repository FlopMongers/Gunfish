using UnityEngine;
using UnityEngine.UI;

public class UIPanel : Image {
    [SerializeField] private Color borderColor = new Color32(0x02, 0x47, 0x8e, 0xFF);
    [SerializeField] private float cornerRadius = 16f;
    [SerializeField] private float borderWidth = 0f;
    [SerializeField] private float edgeSoftness = 1.5f;
    [SerializeField] private bool outlineOnly = false;

    // Shadows Image.preserveAspect: that one only affects sprite rendering, and OnEnable below
    // always clears the sprite, so it has no visible effect and its Inspector control never draws.
    [SerializeField] private new bool preserveAspect = false;
    private float lockedAspectRatio = 1f;
    private float lastWidth;
    private float lastHeight;
    private bool aspectWasLocked;
    private bool applyingAspect;

    private static Shader roundedRectShader;
    private Material materialInstance;

    public Color BorderColor { get => borderColor; set { borderColor = value; Apply(); } }
    public float CornerRadius { get => cornerRadius; set { cornerRadius = Mathf.Max(0f, value); Apply(); } }
    public float BorderWidth { get => borderWidth; set { borderWidth = Mathf.Max(0f, value); Apply(); } }
    public bool OutlineOnly { get => outlineOnly; set { outlineOnly = value; Apply(); } }
    public new bool PreserveAspect { get => preserveAspect; set { preserveAspect = value; SyncAspectLock(); } }

    protected override void OnEnable() {
        base.OnEnable();
        sprite = null;
        type = Image.Type.Simple;

        if (roundedRectShader == null) {
            roundedRectShader = Shader.Find("Gunfish/UI/RoundedRect");
        }
        if (roundedRectShader == null) {
            Debug.LogError("UIPanel: shader 'Gunfish/UI/RoundedRect' not found.");
            return;
        }
        materialInstance = new Material(roundedRectShader);
        material = materialInstance;

        transform.hasChanged = false;
        aspectWasLocked = false;
        SyncAspectLock();
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
        EnforceAspectRatio();
        Apply();
    }

#if UNITY_EDITOR
    protected override void OnValidate() {
        base.OnValidate();
        cornerRadius = Mathf.Max(0f, cornerRadius);
        borderWidth = Mathf.Max(0f, borderWidth);
        edgeSoftness = Mathf.Max(0f, edgeSoftness);
        SyncAspectLock();
        EnforceAspectRatio();
        Apply();
    }

    protected override void Reset() {
        base.Reset();
        color = UITheme.Surface700;
    }
#endif

    private void SyncAspectLock() {
        if (preserveAspect && !aspectWasLocked) {
            Rect rect = rectTransform.rect;
            lastWidth = rect.width;
            lastHeight = rect.height;
            if (lastHeight > 0f) {
                lockedAspectRatio = lastWidth / lastHeight;
            }
        }
        aspectWasLocked = preserveAspect;
    }

    private void EnforceAspectRatio() {
        if (!preserveAspect || applyingAspect || lockedAspectRatio <= 0f) return;

        Rect rect = rectTransform.rect;
        bool widthChanged = !Mathf.Approximately(rect.width, lastWidth);
        bool heightChanged = !Mathf.Approximately(rect.height, lastHeight);
        if (!widthChanged && !heightChanged) return;

        applyingAspect = true;
        if (heightChanged && !widthChanged) {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.height * lockedAspectRatio);
        } else {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.width / lockedAspectRatio);
        }
        applyingAspect = false;

        rect = rectTransform.rect;
        lastWidth = rect.width;
        lastHeight = rect.height;
    }

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
        materialInstance.SetFloat("_OutlineOnly", outlineOnly ? 1f : 0f);
        materialInstance.SetColor("_BorderColor", borderColor);
    }
}
