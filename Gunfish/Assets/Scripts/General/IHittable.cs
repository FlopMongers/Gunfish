using UnityEngine;

public interface IHittable
{
    GameObject gameObject { get; }
    public void Hit(HitObject hitObject) { }
}
