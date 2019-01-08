using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    [SerializeField] private float maxHealth = 100f;

    Transform destroyedObject;
    Renderer rend;

    Color initColor;

    bool alive = true;

    private void Start () {
        destroyedObject = GetComponentInChildren<Explodable> ().transform;
        alive = true;

        health = maxHealth;

        rend = GetComponent<Renderer> ();

        if (rend) {
            initColor = rend.material.color;
        }
    }

    private void Update () {
        if (Input.GetKeyDown (KeyCode.Space)) {
            TakeDamage (30f);
        }
    }

    public void TakeDamage (float damage = 50f) {
        if (!alive)
            return;

        health -= damage;

        if (health <= 0f) {
            health = 0f;
            alive = false;
            Destroy ();
        }

        float lerp = Mathf.Lerp (0.50f, 1f, health / maxHealth);
        Color targColor = new Color (initColor.r * lerp, initColor.g * lerp, initColor.b * lerp);
        rend.material.color = targColor;
    }

    private void Destroy () {
        if (null != destroyedObject) {
            destroyedObject.SetParent (null);
            destroyedObject.GetComponent<Explodable> ().explode ();

            Destroy (destroyedObject.gameObject, 3f);
            foreach (Transform child in destroyedObject) {
                //child.GetComponent<Rigidbody2D>().Ad
                Destroy (child.gameObject, 3f);
            }
        }

        if (destroyedObject != transform) {
            Destroy (gameObject);
        }
    }
}
