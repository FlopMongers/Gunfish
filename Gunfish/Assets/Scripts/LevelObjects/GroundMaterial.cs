using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMaterial : ObjectMaterial
{
    Dictionary<Gunfish, int> fishes = new Dictionary<Gunfish, int>();

    public virtual void AddFish(Gunfish gunfish)
    {

    }

    public virtual void RemoveFish(Gunfish gunfish)
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GunfishSegment segment = collision.collider.GetComponent<GunfishSegment>();
        if (segment != null)
        {
            if (fishes.ContainsKey(segment.gunfish) == false)
            {
                fishes[segment.gunfish] = 0;
            }
            fishes[segment.gunfish] += 1;
            AddFish(segment.gunfish);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        GunfishSegment segment = collision.collider.GetComponent<GunfishSegment>();
        if (segment != null)
        {
            if (fishes.ContainsKey(segment.gunfish) == false)
            {
                return;
            }
            fishes[segment.gunfish] -= 1;
            if (fishes[segment.gunfish] <= 0)
            {
                fishes.Remove(segment.gunfish);
                RemoveFish(segment.gunfish);
            }
        }
    }
}
