using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public CircleCollider2D radius;
    public AnimationCurve damageCurve;
    public float damageScale;

    public PointEffector2D effector;

    // Start is called before the first frame update
    void Start()
    {
        Explode();
        // screen shake
        Invoke("TurnOffEffector", 0.25f);
    }

    void TurnOffEffector() {
        effector.enabled = false;
    }

    void Explode() {
        // circle cast to get things in radius
        // raycast to make sure no walls in the way
        // raycast from the things you hit and if blocked by ground, then ignore
        int mask = ~LayerMask.GetMask("Ground");
        foreach (var hit in Physics2D.CircleCastAll(transform.position, radius.bounds.extents.x, Vector2.zero, 0, mask)) {
            //print($"hit {hit.transform}");
            HashSet<GameObject> hittables = new HashSet<GameObject>();
            GunfishSegment segment = hit.transform.GetComponent<GunfishSegment>();
            Transform hitTransform = hit.transform;
            IHittable hittable = null;
            if (segment != null) {
                hittable = segment.gunfish;
            }
            else if (hitTransform.GetComponentInParent<Shootable>()) { hittable = hitTransform.GetComponentInParent<Shootable>(); }
            if (hittable == null || hittables.Contains(hittable.gameObject)) {
                continue;
            }
            //print(hittable);
            Vector2 dir = hit.point - (Vector2)transform.position;
            if (Physics2D.Raycast(transform.position, (dir).normalized, dir.magnitude, ~mask) == false) {
                float nearness = damageCurve.Evaluate(Vector3.Distance(transform.position, hittable.gameObject.transform.position) / radius.bounds.extents.x);
                //print(nearness);
                // calculate hit object based on distance
                float damage = nearness * damageScale;
                if (hittable is Gunfish) {
                    Gunfish gunfish = (Gunfish)hittable;
                    gunfish.Hit(new FishHitObject(
                        gunfish.MiddleSegmentIndex,
                        gunfish.MiddleSegment.transform.position,
                        (gunfish.MiddleSegment.transform.position - transform.position).normalized,
                        gameObject,
                        damage,
                        0,
                        HitType.Explosive));
                }
                else {
                    hittable.Hit(new HitObject(
                        hittable.gameObject.transform.position,
                        (hittable.gameObject.transform.position - transform.position).normalized,
                        hittable.gameObject,
                        damage,
                        0,
                        HitType.Explosive,
                        ignoreMass: true));
                }
                hittables.Add(hittable.gameObject);
            }
        }
    }
}
