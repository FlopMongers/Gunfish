using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishSelectPanel : MonoBehaviour {
    public enum State {
        Inactive,
        Selecting,
        Confirmed,
    }
    
    public Image fishImage;
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public RectTransform confirmHint;
    public RectTransform cancelHint;
    public RectTransform readyHint;
    public RectTransform disabledHint;

    [SerializeField] private State state;

    [SerializeField] private bool arrowsActive;
    private float arrowsT;
    private AnimationCurve tween;
    private Vector2 leftPosition;
    private Vector2 rightPosition;
    [SerializeField] private float arrowAnimationDuration = 1f;

    private void Start() {
        InitializeArrows();
        SetState(State.Inactive);
    }
    private void Update() {
        AnimateArrows();

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (state == State.Inactive) {
                SetState(State.Selecting);
            } else if (state == State.Selecting) {
                SetState(State.Confirmed);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (state == State.Confirmed) {
                SetState(State.Selecting);
            } else if (state == State.Selecting) {
                SetState(State.Inactive);
            }
        }
    }

    private void InitializeArrows() {
        arrowsActive = false;
        arrowsT = 0f;
        tween = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        leftPosition = leftArrow.anchoredPosition;
        rightPosition = rightArrow.anchoredPosition;
    }

    private void AnimateArrows() {
        // Animate
        var t = tween.Evaluate(arrowsT);

        leftArrow.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, t);
        rightArrow.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, t);

        // Transition
        if (arrowsActive) {
            arrowsT += Time.deltaTime / arrowAnimationDuration;
        } else {
            arrowsT -= Time.deltaTime / arrowAnimationDuration;
        }
        arrowsT = Mathf.Clamp01(arrowsT);
    }

    public void SetFishImage(Sprite sprite) {
        fishImage.sprite = sprite;
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
        arrowsActive = false;
        confirmHint.gameObject.SetActive(false);
        cancelHint.gameObject.SetActive(false);
        readyHint.gameObject.SetActive(false);
        disabledHint.gameObject.SetActive(true);
    }

    private void SetStateSelecting() {
        arrowsActive = false;
        confirmHint.gameObject.SetActive(true);
        cancelHint.gameObject.SetActive(true);
        readyHint.gameObject.SetActive(false);
        disabledHint.gameObject.SetActive(false);
    }

    private void SetStateConfirmed() {
        arrowsActive = true;
        confirmHint.gameObject.SetActive(false);
        cancelHint.gameObject.SetActive(true);
        readyHint.gameObject.SetActive(true);
        disabledHint.gameObject.SetActive(false);
    }
}
