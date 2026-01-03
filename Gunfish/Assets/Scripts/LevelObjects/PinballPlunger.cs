using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ObjectMaterial))]
[RequireComponent(typeof(FishDetector))]
public class PinballPlunger : MonoBehaviour
{
    [SerializeField] private Transform buttonTop;
    [SerializeField] private FishDetector fishDetector;
    [SerializeField] private CompositeCollisionDetector fishCollisionDetector;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpringJoint2D springJoint;

    private bool animating = false;

    private float lockPosition = -0.5f;
    private float releasePosition = -2.0f;

    [SerializeField] [Range(0.0f, 10.0f)] float releaseDelay = 1f;
    [SerializeField] [Range(0.0f, 10.0f)] float releaseHold = 2f;
    [SerializeField] [Range(0.0f, 10.0f)] float lockDuration = 5f;

    private void Start() {
        fishCollisionDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) {
            if (!collision.collider.GetComponent<ObjectMaterial>()) return;
            var allowedMaterials = new List<MaterialType> {
                MaterialType.Fish,
                MaterialType.Rock,
            };
            if (allowedMaterials.Contains(collision.collider.GetComponent<ObjectMaterial>().materialType)) {
                LaunchSequence();
            }
        };
        Lock();
    }

    private void LaunchSequence() {
        if (animating) return;
        animating = true;
        StartCoroutine(LaunchSequenceCR());
    }

    private IEnumerator LaunchSequenceCR() {
        print($"Waiting {releaseDelay} seconds to release...");
        yield return new WaitForSeconds(releaseDelay);
        Release();
        yield return new WaitForSeconds(releaseHold);
        Lock();
        yield return new WaitForSeconds(lockDuration);
        animating = false;
    }

    public void Lock() {
        springJoint.frequency = 1.5f;
        springJoint.connectedAnchor = Vector2.up * lockPosition;
    }

    public void Release() {
        audioSource?.Play();
        springJoint.frequency = 10f;
        springJoint.connectedAnchor = Vector2.up * releasePosition;
    }
}
