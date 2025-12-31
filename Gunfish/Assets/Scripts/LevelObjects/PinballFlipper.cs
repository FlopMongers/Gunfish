using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ObjectMaterial))]
[RequireComponent(typeof(FishDetector))]
public class PinballFlipper : MonoBehaviour
{
    [SerializeField] private FishDetector fishDetector;
    [SerializeField] private CompositeCollisionDetector fishCollisionDetector;
    [SerializeField] private AudioSource releaseAudioSource;
    [SerializeField] private AudioSource lockAudioSource;
    [SerializeField] private Vector2 triggerPositions = new Vector2(-30f, 30f);


    [SerializeField] Vector2 releaseDelay = Vector2.up;
    [SerializeField] [Range(0.0f, 10.0f)] float switchDuration = 0.25f;
    [SerializeField] Vector2 resetDelay = Vector2.up;

    private bool animating = false;

    private void Start() {
        transform.eulerAngles = new Vector3(0f, 0f, triggerPositions.x);
        fishCollisionDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) {
            print("Collision Detected");
            print("Collision Source: " + src.name);
            if (src.GetComponent<GunfishSegment>() != null) {
                print("Has Gunfish segment");
                LaunchSequence();
            }
        };
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            LaunchSequence();
        }
    }

    private void LaunchSequence() {
        if (animating) return;
        DOTween.Sequence()
            .AppendInterval(Random.Range(releaseDelay.x, releaseDelay.y))
            .AppendCallback(() => { releaseAudioSource?.Play(); })
            .Append(GetComponent<Rigidbody2D>().DORotate(triggerPositions.y, switchDuration).SetEase(Ease.OutQuad))
            .AppendInterval(Random.Range(resetDelay.x, resetDelay.y))
            .AppendCallback(() => { lockAudioSource?.Play(); })
            .Append(GetComponent<Rigidbody2D>().DORotate(triggerPositions.x, switchDuration).SetEase(Ease.InQuad))
            .OnComplete(() => { animating = false; })
            .Play();
    }

    private void OnDrawGizmos() {
        // draw an arc showing the flipper movement
        Color targetColor = Color.cyan;
        Gizmos.color = targetColor;
        Vector3 center = transform.position;
        float radius = 1.0f;
        int segments = 20;
        Vector3 previousPoint = center + Quaternion.Euler(0f, 0f, triggerPositions.x) * Vector3.right * radius;
        Gizmos.DrawLine(center, previousPoint);
        
        for (int i = 1; i <= segments; i++) {
            float t = (float)i / (float)segments;
            Gizmos.color = Color.Lerp(Color.cyan, Color.yellow, t);
            float angle = Mathf.Lerp(triggerPositions.x, triggerPositions.y, t);
            Vector3 currentPoint = center + Quaternion.Euler(0f, 0f, angle) * Vector3.right * radius * transform.localScale.x;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        Gizmos.DrawLine(center, previousPoint);
    }
}
