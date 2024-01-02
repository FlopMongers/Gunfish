using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toaster : Shootable
{
    public WaterInteractor waterInteractor;

    public bool zapping;

    public float zapWaitTimer;
    Vector2 zapWaitTimerRange = new Vector2(6f, 10f);
    int waterMask = 0;

    public GameObject zapPrefab;
    public GameObject zapInstance;

    public GameObject toastPrefab;
    float toastForce = 1f;
    Collider2D toastInstance;
    public Collider2D ownCollider;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        waterMask = LayerMask.GetMask("Water");
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!damaged) {
            if (waterInteractor.isUnderwater > 0) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 1f, waterMask);
                if (hit.collider != null)
                    UpdateHealth(-health);
            }
            return;
        }

        zapWaitTimer -= Time.deltaTime;
        if (zapWaitTimer <= 0 && !zapping) {
            if (waterInteractor.isUnderwater > 0) {
                // instantiate zapper
                if (zapPrefab != null) {
                    zapInstance = Instantiate(zapPrefab, transform.position, Quaternion.identity);
                    zapInstance.transform.parent = transform;
                }
            }
        }
        if (zapInstance != null) {
            zapping = true;
        }
        if (zapping == true && zapInstance == null) {
            zapWaitTimer = Random.Range(zapWaitTimerRange.x, zapWaitTimerRange.y);
            zapping = false;
        }
    }

    protected override void Damage() {
        base.Damage();
        zapWaitTimer = 0;
        // instantiate toast
        if (toastInstance != null)
            return;
        toastInstance = Instantiate(toastPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, transform.up)).GetComponentInChildren<Collider2D>();
        toastInstance.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(transform.up * toastForce, ForceMode2D.Impulse);
        Physics2D.IgnoreCollision(ownCollider, toastInstance, true);
        Invoke("CollideToast", 1f);
    }

    void CollideToast() {
        if (toastInstance != null) {
            Physics2D.IgnoreCollision(ownCollider, toastInstance.GetComponent<Collider2D>(), false);
        }
    }
}
