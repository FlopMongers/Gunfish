using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunfish : MonoBehaviour
{
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpTorque = 30f;

    [SerializeField] 
    [Range(0f, 90f)] 
    private float jumpAngleFromHorizontal = 45f;

    private float maxJumpCD = 1f;
    [SerializeField] private float curJumpCD;

    private float maxShotCD = 1f;
    [SerializeField] private float curShotCD;

    private Vector2 angle;

    private Rigidbody2D rb;

    private void Start () {
        rb = GetComponent<Rigidbody2D> ();
        curJumpCD = 0f;
        curShotCD = 0f;
    }

    private void FixedUpdate () {
        CooldownHandler ();

        Move (Input.GetAxis ("Horizontal"),
            Input.GetKeyDown (KeyCode.Space));
    }

    private void CooldownHandler () {
        if (curJumpCD > 0f) {
            curJumpCD -= Time.unscaledDeltaTime;
        } else {
            curJumpCD = 0f;
        }

        if (curShotCD > 0f) {
            curShotCD -= Time.unscaledDeltaTime;
        } else {
            curShotCD = 0f;
        }
    }

    public void Move (float flop, bool shooting) {
        if (null == rb) {
            if (null == (rb = GetComponent<Rigidbody2D> ())) {
                Debug.LogError ("No Rigidbody component attached to Gunfish!");
                return;
            }
        }

        if (Mathf.Abs (flop) > float.Epsilon && curJumpCD < float.Epsilon) {
            float direction = Mathf.Sign (flop);

            angle = new Vector2 (Mathf.Cos (jumpAngleFromHorizontal * Mathf.Deg2Rad),
                                 Mathf.Sin (jumpAngleFromHorizontal * Mathf.Deg2Rad)).normalized;
            angle.x *= direction;

            foreach (Transform child in transform) {
                if (child.localPosition.x * direction > 0f)
                    continue;

                Rigidbody2D childRB;
                if (null != (childRB = child.GetComponent<Rigidbody2D> ())) {
                    childRB.AddForce (angle * jumpForce / transform.childCount);
                }
            }


            rb.AddTorque (jumpTorque * direction);

            curJumpCD = maxJumpCD;
        }

        if (true == shooting && curShotCD < float.Epsilon) {
            Debug.Log ("<color=red>BANG!</color>");
            curShotCD = maxShotCD;
        }
    }
}
