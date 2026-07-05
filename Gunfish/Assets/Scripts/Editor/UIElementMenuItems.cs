using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class UIElementMenuItems {
    private const string UILayerName = "UI";
    private static readonly Vector2 PanelSize = new Vector2(160f, 160f);
    private static readonly Vector2 ButtonSize = new Vector2(160f, 30f);

    [MenuItem("GameObject/UI (Canvas)/Gunfish/UI Panel", false, 10)]
    public static void CreateUIPanel(MenuCommand menuCommand) {
        var go = ObjectFactory.CreateGameObject("UI Panel", typeof(UIPanel));
        go.GetComponent<RectTransform>().sizeDelta = PanelSize;

        var panel = go.GetComponent<UIPanel>();
        panel.color = UITheme.Surface700;
        panel.CornerRadius = 16f;

        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI (Canvas)/Gunfish/UI Button", false, 11)]
    public static void CreateUIButton(MenuCommand menuCommand) {
        var go = ObjectFactory.CreateGameObject("UI Button", typeof(UIButton));
        go.GetComponent<RectTransform>().sizeDelta = ButtonSize;

        var panel = go.GetComponent<UIPanel>();
        panel.color = UITheme.Surface500;
        panel.CornerRadius = 8f;

        var labelGo = ObjectFactory.CreateGameObject("Label", typeof(TextMeshProUGUI));
        SetParentAndAlign(labelGo, go);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "Button";
        label.alignment = TextAlignmentOptions.Center;
        if (UITheme.ButtonLabel.Font != null) {
            label.font = UITheme.ButtonLabel.Font;
            label.fontSize = UITheme.ButtonLabel.Size;
            label.fontStyle = UITheme.ButtonLabel.Style;
        }

        var serializedButton = new SerializedObject(go.GetComponent<UIButton>());
        serializedButton.FindProperty("label").objectReferenceValue = label;
        serializedButton.ApplyModifiedProperties();

        PlaceUIElementRoot(go, menuCommand);
    }

    // Mirrors UnityEditor.UI.MenuOptions (internal to the UGUI package, so
    // not directly callable) - same canvas/prefab-stage placement behavior
    // as Unity's own GameObject/UI (Canvas)/* menu items.
    private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand) {
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null) {
            parent = GetOrCreateCanvasGameObject();

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent)) {
                parent = prefabStage.prefabContentsRoot;
            }
        }
        if (parent.GetComponentsInParent<Canvas>(true).Length == 0) {
            GameObject canvas = CreateNewUI();
            Undo.SetTransformParent(canvas.transform, parent.transform, "");
            parent = canvas;
        }

        GameObjectUtility.EnsureUniqueNameForSibling(element);
        SetParentAndAlign(element, parent);

        Undo.RegisterFullObjectHierarchyUndo(parent, "");
        Undo.SetCurrentGroupName("Create " + element.name);

        Selection.activeGameObject = element;
    }

    private static void SetParentAndAlign(GameObject child, GameObject parent) {
        if (parent == null) return;

        Undo.SetTransformParent(child.transform, parent.transform, "");

        var rectTransform = child.transform as RectTransform;
        if (rectTransform != null) {
            rectTransform.anchoredPosition = Vector2.zero;
            Vector3 localPosition = rectTransform.localPosition;
            localPosition.z = 0;
            rectTransform.localPosition = localPosition;
        } else {
            child.transform.localPosition = Vector3.zero;
        }
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        SetLayerRecursively(child, parent.layer);
    }

    private static void SetLayerRecursively(GameObject go, int layer) {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++) {
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }

    private static GameObject GetOrCreateCanvasGameObject() {
        GameObject selectedGo = Selection.activeGameObject;

        Canvas canvas = selectedGo != null ? selectedGo.GetComponentInParent<Canvas>() : null;
        if (IsValidCanvas(canvas)) return canvas.gameObject;

        Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
        for (int i = 0; i < canvasArray.Length; i++) {
            if (IsValidCanvas(canvasArray[i])) return canvasArray[i].gameObject;
        }

        return CreateNewUI();
    }

    private static bool IsValidCanvas(Canvas canvas) {
        if (canvas == null || !canvas.gameObject.activeInHierarchy) return false;
        if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0) return false;
        return StageUtility.GetStageHandle(canvas.gameObject) == StageUtility.GetCurrentStageHandle();
    }

    private static GameObject CreateNewUI() {
        var root = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.layer = LayerMask.NameToLayer(UILayerName);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        StageUtility.PlaceGameObjectInCurrentStage(root);
        bool customScene = false;
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null) {
            Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");
            customScene = true;
        }

        Undo.SetCurrentGroupName("Create " + root.name);

        if (!customScene) {
            CreateEventSystem();
        }
        return root;
    }

    private static void CreateEventSystem() {
        StageHandle stage = StageUtility.GetCurrentStageHandle();
        var eventSystem = stage.FindComponentOfType<EventSystem>();
        if (eventSystem == null) {
            var eventSystemGo = ObjectFactory.CreateGameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            StageUtility.PlaceGameObjectInCurrentStage(eventSystemGo);
        }
    }
}
