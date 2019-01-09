using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour {

    [SerializeField] private float fallAccel = 9.81f;
    [SerializeField] private float riseSpeed = 1f;
    [SerializeField] private float secondsToWaitRaised = 3f;
    [SerializeField] private float secondsToWaitFallen = 3f;

    [SerializeField] private bool active = true;
    private bool activeLF;
    private bool anchorsAweighing;

    [SerializeField] private float distanceToGround;
    private SpriteRenderer sr;

    private RaycastHit2D hit;
    private Rigidbody2D rb;

    private SliderJoint2D slider;

    private LineRenderer chain;
    private Transform chainTransform;

    private Transform anchorBase;
    private ParticleSystem hitParticles;

    // Start is called before the first frame update
    void Start () {
        activeLF = active;
        anchorsAweighing = false;

        anchorBase = transform.Find ("Base");
        hitParticles = anchorBase.GetComponent<ParticleSystem> ();

        sr = GetComponent<SpriteRenderer> ();
        if (null == sr) {
            Debug.LogError ("Anchor does not have active sprite renderer!");
            return;
        }

        rb = GetComponent<Rigidbody2D> ();
        if (null == rb) {
            Debug.LogError ("Anchor does not have active Rigidbody2D!");
            active = false;
        }

        slider = GetComponent<SliderJoint2D> ();
        if (null == slider) {
            slider = gameObject.AddComponent<SliderJoint2D> ();
        }

        chainTransform = transform.Find ("Chain");
        if (null == chainTransform) {
            Debug.LogError ("Anchor does not have a chain component!");
        } else {
            chain = GetComponentInChildren<LineRenderer> ();
            chain.useWorldSpace = true;
            chain.positionCount = 2;

            //Chain is flipped 180 degrees, so position 1 is on top.
            chain.SetPosition (1, chainTransform.position + Vector3.up * 0.4f);
            chain.SetPosition (0, chainTransform.position);
        }

        slider.connectedAnchor = transform.position;
        slider.angle = 90f;

        Vector3 basePosition = transform.position - transform.up * 
        sr.sprite.texture.height / sr.sprite.pixelsPerUnit * 
        transform.localScale.y / 2f;
        //Vector3 basePosition = anchorBase.position;

        hit = Physics2D.Raycast (basePosition, -transform.up);

        if (hit) {
            distanceToGround = hit.distance;
            JointTranslationLimits2D limits = new JointTranslationLimits2D ();
            limits.min = 0f;
            limits.max = distanceToGround;
            slider.limits = limits;
            slider.useLimits = true;
            StartCoroutine (AnchorsAweigh ());
        }
    }

    private void Update () {
        if (active && !activeLF) {
            if (anchorsAweighing) {
                StartCoroutine (AnchorsAweigh ());
            }
        }

        activeLF = active;
    }

    IEnumerator AnchorsAweigh () {
        anchorsAweighing = true;
        while (active) {
            yield return new WaitForSeconds (secondsToWaitRaised);

            float distanceTravelled = 0f;

            JointMotor2D motor = new JointMotor2D ();
            motor.maxMotorTorque = 10000;
            motor.motorSpeed = 0f;
            slider.motor = motor;

            while (distanceTravelled < distanceToGround) {
                distanceTravelled += motor.motorSpeed * Time.deltaTime;
                motor.motorSpeed += fallAccel * Time.deltaTime;
                slider.motor = motor;

                if (null != chain) {
                    chain.SetPosition (0, chainTransform.position);
                }

                yield return new WaitForEndOfFrame ();
            }

            if (null != hitParticles) {
                hitParticles.Play ();
            }

            motor.motorSpeed = 0f;
            slider.motor = motor;
            yield return new WaitForSeconds (secondsToWaitFallen);
            motor.motorSpeed = -riseSpeed;
            slider.motor = motor;

            distanceTravelled = 0f;

            while (distanceTravelled < distanceToGround) {
                distanceTravelled += riseSpeed * Time.deltaTime;

                if (null != chain) {
                    chain.SetPosition (0, chainTransform.position);
                }

                yield return new WaitForEndOfFrame ();
            }
        }
        anchorsAweighing = false;
    }
}
