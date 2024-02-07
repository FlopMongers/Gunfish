using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Destroyer))]
public class BassballBall : MonoBehaviour
{
    public Destroyer destroyer;

    private void Start() {
        destroyer = destroyer ?? GetComponent<Destroyer>();
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (destroyer.destroying)
            return;
        Goal goal = collision.attachedRigidbody?.GetComponent<Goal>();
        if (goal != null && collision.OverlapPoint(transform.position)) {
            goal.OnGoal?.Invoke(goal, this);
            destroyer.GETTEM();
        }
    }
}
