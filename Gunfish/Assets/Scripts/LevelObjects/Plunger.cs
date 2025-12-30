using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(ObjectMaterial))]
[RequireComponent(typeof(FishDetector))]
public class Plunger : MonoBehaviour
{
    [SerializeField] private Transform buttonTop;
    [SerializeField] private FishDetector fishDetector;
    [SerializeField] private CompositeCollisionDetector fishCollisionDetector;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpringJoint2D springJoint;

    private enum PlungerState {
        Locking,
        Locked,
        Launching,
        Launched,
    }
    [SerializeField] private PlungerState currentState = PlungerState.Locked;
    private bool animating = false;

    private float lockPosition = -0.5f;
    private float releasePosition = -2.0f;

    [SerializeField] [Range(0.0f, 10.0f)] float releaseDelay = 1f;
    [SerializeField] [Range(0.0f, 10.0f)] float releaseHold = 2f;
    [SerializeField] [Range(0.0f, 10.0f)] float lockDuration = 5f;

    private void Start() {
        currentState = PlungerState.Launched;
        fishCollisionDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) {
            LaunchSequence();
        };
        Lock();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            LaunchSequence();
        }
    }

    private void LaunchSequence() {
        if (animating) return;
        animating = true;
        StartCoroutine(LaunchSequenceCR());
    }

    private IEnumerator LaunchSequenceCR() {
        print("Plunger Launch Sequence Started");
        print($"Waiting {releaseDelay} seconds to release...");
        yield return new WaitForSeconds(releaseDelay);
        print("Releasing plunger!");
        Release();
        print($"Holding for {releaseHold} seconds...");
        yield return new WaitForSeconds(releaseHold);
        print("Locking plunger!");
        Lock();
        print($"Plunger locked for {lockDuration} seconds.");
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
