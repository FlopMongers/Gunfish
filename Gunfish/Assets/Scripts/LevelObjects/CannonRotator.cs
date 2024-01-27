using UnityEngine;
using DG.Tweening;

public class CannonRotator : MonoBehaviour {
    public AnimationCurve animCurve;

    // Adjust these variables to control the start and stop rotation angles
    public float startRotation = 0f;
    public float stopRotation = 180f;

    // Adjust this variable to control the rotation speed
    public float rotationSpeed = 1f;

    // Adjust this variable to control how quickly the rotation slows down
    public float slowDownFactor = 1.5f;

    void Start() {
        startRotation = transform.rotation.eulerAngles.z + startRotation;
        stopRotation = transform.rotation.eulerAngles.z + stopRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, startRotation);
        // Call the RotateBarrel method to start the rotation
        RotateBarrel();
    }

    void RotateBarrel() {
        // Rotate the barrel between startRotation and stopRotation using DOTween for smooth animation
        transform.DORotate(new Vector3(0f, 0f, stopRotation), rotationSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(animCurve);
            //brian griffin;
    }
}