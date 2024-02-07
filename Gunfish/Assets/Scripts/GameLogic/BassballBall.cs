using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Destroyer))]
public class BassballBall : MonoBehaviour
{
    public Destroyer destroyer;
    public Shootable shootable;

    private void Start() {
        destroyer = destroyer ?? GetComponent<Destroyer>();
        shootable = shootable ?? GetComponent<Shootable>();
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (shootable.dead == true)
            return;
        Goal goal = collision.GetComponentInParent<Goal>();
        if (goal != null && collision.OverlapPoint(transform.position)) {
            goal.OnGoal?.Invoke(goal, this);
            shootable.undamageable = false;
            shootable.indestructible = false;
            shootable.UpdateHealth(-shootable.health);
        }
    }
}
