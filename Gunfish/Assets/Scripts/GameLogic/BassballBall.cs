using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Destroyer))]
public class BassballBall : MonoBehaviour
{
    public Destroyer destroyer;
    public Shootable shootable;
    // Set the moment a goal is scored; Shootable.dead only flips on the next Update(),
    // so extra physics ticks in between would score the same ball again.
    private bool scored;

    private void Start() {
        destroyer = destroyer ?? GetComponent<Destroyer>();
        shootable = shootable ?? GetComponent<Shootable>();
        FindAnyObjectByType<CinemachineTargetGroup>().AddMember(transform, 1, 1);
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (scored || shootable.dead == true)
            return;
        Goal goal = collision.GetComponentInParent<Goal>();
        if (goal != null && collision.OverlapPoint(transform.position)) {
            scored = true;
            goal.OnGoal?.Invoke(goal, this);
            shootable.undamageable = false;
            shootable.indestructible = false;
            shootable.UpdateHealth(-shootable.health);
        }
    }
}
