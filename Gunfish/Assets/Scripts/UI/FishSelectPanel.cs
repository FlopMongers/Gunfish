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
    public Image outline;
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


    private void Start() {
        initialized = false;
        Initialize();
    }

    public void Initialize() {
        if (initialized)
            return;
        SetState(State.Inactive);
        initialized = true;
        // var left = leftArrow.anchoredPosition;
        // var right = rightArrow.anchoredPosition;
        // leftArrow.anchoredPosition = right;
        // rightArrow.anchoredPosition = left;
        // DOTween.Sequence()
        //     .Append(leftArrow.DOAnchorPosX(left.x, arrowAnimationDuration))
        //     .Join(rightArrow.DOAnchorPosX(right.x, arrowAnimationDuration))
        //     .Play();
    }

    public void SetFishImage(Sprite sprite) {
        fishImage.color = Color.white;
        fishImage.sprite = sprite;
        fishImage.preserveAspect = true;
    }

    public void SetColor(Color color) {
        outline.color = color;
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
