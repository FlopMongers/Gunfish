using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishSelectPanel : MonoBehaviour {
    public enum State {
        Inactive,
        Selecting,
        Confirmed,
    }
    
    public Image fishImage;
    public UIPanel outline;
    public TMP_Text description;
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public RectTransform confirmHint;
    public RectTransform cancelHint;
    public RectTransform readyHint;
    public RectTransform disabledHint;

    public State state { get; private set; }

    [Range(0.2f, 2f)] public float arrowAnimationDuration = 0.5f;

    private bool initialized;
    private Vector3 leftArrowStartPos;
    private Vector3 rightArrowStartPos;
    private float readyT;


    private void Start() {
        initialized = false;
        Initialize();
    }

    private void Update() {
        var delta = Time.deltaTime / arrowAnimationDuration;
        if (state == State.Confirmed) {
            readyT += delta;
        } else {
            readyT -= delta;
        }
        readyT = Mathf.Clamp01(readyT);
        // cubic bezier easing function
        var easedT = -(Mathf.Cos(Mathf.PI * readyT) - 1) / 2;
        leftArrow.anchoredPosition = Vector3.Lerp(leftArrowStartPos, rightArrowStartPos, easedT);
        rightArrow.anchoredPosition = Vector3.Lerp(rightArrowStartPos, leftArrowStartPos, easedT);
    }

    public void Initialize() {
        if (initialized)
            return;
        SetState(State.Inactive);
        readyT = 0f;
        leftArrowStartPos = leftArrow.anchoredPosition;
        rightArrowStartPos = rightArrow.anchoredPosition;
        initialized = true;
    }

    public void SetFishImage(Sprite sprite) {
        fishImage.color = Color.white;
        fishImage.sprite = sprite;
        fishImage.preserveAspect = true;
    }

    public void SetColor(Color color) {
        outline.BorderColor = color;
    }

    public void Right() {
        rightArrow.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 1);
    }

    public void Left() {
        leftArrow.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 1);
    }

    public void SetFishDescription(string text) {
        description.SetText(text);
    }


    public void SetState(State state) {
        this.state = state;
        switch (state) {
            case State.Inactive:
                SetStateInactive();
                break;
            case State.Selecting:
                SetStateSelecting();
                break;
            case State.Confirmed:
                SetStateConfirmed();
                break;
        }
    }

    private void SetStateInactive() {
        fishImage.color = Color.black;
        description.SetText("");
        confirmHint.gameObject.SetActive(false);
        cancelHint.gameObject.SetActive(false);
        readyHint.gameObject.SetActive(false);
        disabledHint.gameObject.SetActive(true);
    }

    private void SetStateSelecting() {
        confirmHint.gameObject.SetActive(true);
        cancelHint.gameObject.SetActive(true);
        readyHint.gameObject.SetActive(false);
        disabledHint.gameObject.SetActive(false);
    }

    private void SetStateConfirmed() {
        confirmHint.gameObject.SetActive(false);
        cancelHint.gameObject.SetActive(true);
        readyHint.gameObject.SetActive(true);
        disabledHint.gameObject.SetActive(false);
    }
}
