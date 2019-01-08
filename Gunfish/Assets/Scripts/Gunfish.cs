using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void FireEvent();

public class Gunfish : MonoBehaviour, IHittable {
    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private float jumpTorque = 1000f;

    [SerializeField]
    [Range(0f, 90f)]
    private float jumpAngleFromHorizontal = 45f;

    private float maxJumpCD = 1f;
    [SerializeField] private float curJumpCD;

    private float maxFireCD = 1f;
    [SerializeField] private float curFireCD;

    private float maxStunCD = 3f;
    [SerializeField] private float curStunCD;
    private int isStunned = 0;

    private float maxSwimCD = 0.25f;
    [SerializeField] private float curSwimCD;

    Gun gun;
    public event FireEvent FireEvent;

    private Vector2 angle;

    private Rigidbody2D rb;

    private void Start() {
        gun = GetComponentInChildren<Gun>();
        if (gun)
            gun.FireSubscribe(this);

        rb = GetComponent<Rigidbody2D>();
        curJumpCD = 0f;
        curFireCD = 0f;
    }

    private void Update() {
        CheckCoolDowns();
    }

    private void CDUpdate(ref float CD, float maxCD, bool stun = false) {
        if (float.IsNaN(CD)) {
            return;
        }

        if (CD > maxCD) {
            CD = maxCD;
        }
        else if (CD > 0f)
            CD -= Time.deltaTime;
        else {
            if (stun && isStunned > 0) {
                isStunned--;
            }
            CD = float.NaN;
        }
    }

    private void CheckCoolDowns() {
        CDUpdate(ref curJumpCD, maxJumpCD);
        CDUpdate(ref curFireCD, maxFireCD);
        CDUpdate(ref curSwimCD, maxSwimCD);
        CDUpdate(ref curStunCD, float.PositiveInfinity, true);
    }


    public void Fire() {
        if (null == rb) {
            if (null == (rb = GetComponent<Rigidbody2D>())) {
                Debug.LogError("No Rigidbody component attached to Gunfish!");
                return;
            }
        }

        if (isStunned < 1 && float.IsNaN(curFireCD) && null != FireEvent) {
            FireEvent();
            Knockback(gun.transform.right, gun.gunInfo.kickback);
            curFireCD = maxFireCD;
        }
    }

    public void Move(int dir) {
        if (null == rb) {
            if (null == (rb = GetComponent<Rigidbody2D>())) {
                Debug.LogError("No Rigidbody component attached to Gunfish!");
                return;
            }
        }

        if (isStunned < 1 && float.IsNaN(curJumpCD)) {

            angle = new Vector2(Mathf.Cos(jumpAngleFromHorizontal * Mathf.Deg2Rad),
                                 Mathf.Sin(jumpAngleFromHorizontal * Mathf.Deg2Rad)).normalized;
            angle.x *= dir;

            foreach (Transform child in transform) {
                if (child.localPosition.x * dir > 0f)
                    continue;

                Rigidbody2D childRB;
                if (null != (childRB = child.GetComponent<Rigidbody2D>())) {
                    childRB.AddForce(angle * jumpForce / transform.childCount);
                }
            }

            rb.AddTorque(jumpTorque * dir);

            curJumpCD = maxJumpCD;
        }
    }

    public void Hit(Vector2 direction, GunInfo gunInfo) {
        Knockback(direction, gunInfo.knockback);
        Stun(gunInfo.stunTime);
        //Check gamemode, if race, then call Stun(), else call Damage()
    }

    public void Knockback(Vector2 direction, float knockback) {
        rb.AddForce(direction * knockback);
    }

    public void Stun(float stunTime) {
        //Oh no you got stunned
        if (float.IsNaN(curStunCD))
            isStunned++;
        curStunCD = stunTime;
    }
}
