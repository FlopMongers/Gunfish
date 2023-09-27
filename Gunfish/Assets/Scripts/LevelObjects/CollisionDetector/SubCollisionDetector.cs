using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCollisionDetector : MonoBehaviour
{
    public CollisionEvent OnComponentCollideEnter, OnComponentCollideExit;
    public TriggerEvent OnComponentTriggerEnter, OnComponentTriggerExit;

    public CompositeCollisionDetector parentDetector;


    private void OnCollisionEnter2D(Collision2D collision) {
        OnComponentCollideEnter?.Invoke(gameObject, collision);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        OnComponentCollideExit?.Invoke(gameObject, collision);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        OnComponentTriggerEnter?.Invoke(gameObject, collision);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        OnComponentTriggerExit?.Invoke(gameObject, collision);
    }
}
