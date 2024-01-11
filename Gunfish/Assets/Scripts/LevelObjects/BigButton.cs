using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Linq;

[RequireComponent(typeof(ObjectMaterial))]
[RequireComponent(typeof(FishDetector))]
public class BigButton : MonoBehaviour
{
    public UnityEvent OnTrigger;

    public bool detectAllCollisions;
    public bool shootable;

    public float impulseThreshold = 2f;

    public CompositeCollisionDetector compositeCollisionDetector;
    public ObjectMaterial objMaterial;
    public FishDetector fishDetector;

    public Transform buttonTop, buttonTopPosition, buttonBottomPosition;

    bool triggered;

    public float triggerDelay = 5f;
    float depressTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        if (detectAllCollisions) {
            compositeCollisionDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) {
                print(collision.relativeVelocity);
                if (collision.relativeVelocity.magnitude > impulseThreshold) {
                    Trigger();
                }
            };
        }
        if (shootable) {
            objMaterial.OnShot += Trigger;
        }
        fishDetector.OnFishCollideEnter += delegate (GunfishSegment segment, Collision2D collision) { Trigger(); };
    }

    void Trigger() {
        // if depressed, don't trigger
        if (triggered)
            return;
        triggered = true;
        buttonTop.DOMove(buttonBottomPosition.position, depressTime).SetEase(Ease.Linear).OnComplete(FinishButtonDepress);
        OnTrigger?.Invoke();
    }

    void FinishButtonDepress() {
        buttonTop.DOMove(buttonTopPosition.position, depressTime).SetEase(Ease.Linear).SetDelay(triggerDelay).OnComplete(NotTriggered);
    }

    void NotTriggered() {
        triggered = false;
    }
}
