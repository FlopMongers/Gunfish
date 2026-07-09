using System.Collections.Generic;
using UnityEngine;

public class Umbrella : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private float bounceCooldown = 1f;
    [SerializeField] private float maxBounceAngle = 80f;

    private readonly Dictionary<Gunfish, float> lastBounceTime = new Dictionary<Gunfish, float>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GunfishSegment segment = collision.collider.GetComponent<GunfishSegment>();
        if (segment == null)
            return;

        bool hitFromAbove = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // contact.normal points away from collision.collider (the fish), so negate it
            // to get the umbrella surface's own normal at the contact point.
            Vector2 umbrellaNormal = -contact.normal;
            if (Vector2.Angle(Vector2.up, umbrellaNormal) <= maxBounceAngle)
            {
                hitFromAbove = true;
                break;
            }
        }


        if (!hitFromAbove)
            return;

        Gunfish gunfish = segment.gunfish;
        if (lastBounceTime.TryGetValue(gunfish, out float lastTime) && Time.time - lastTime < bounceCooldown)
            return;
        lastBounceTime[gunfish] = Time.time;

        for (int i = 0; i < gunfish.segments.Count; i++)
        {
            gunfish.body.ApplyForceToSegment(i, Vector2.up * bounceForce, ForceMode2D.Impulse);
        }
    }
}
