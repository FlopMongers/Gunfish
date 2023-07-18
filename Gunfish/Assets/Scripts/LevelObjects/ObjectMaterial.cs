using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMaterial : MonoBehaviour
{
    public bool skipCollision;
    public MaterialType materialType;

    // on collision, if not external collision and not ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (skipCollision)
            return;

        // if other has no thingy, play send to handler with default
        ObjectMaterial mat = collision.collider.GetComponent<ObjectMaterial>();
        if (mat == null)
        {
            FX_CollisionHandler.instance?.HandleDefaultCollision(this, collision);
            return;
        }
        // if other skip collision, send the collision to the fx handler
        if (mat.skipCollision == true || materialType > mat.materialType || (materialType == mat.materialType && GetInstanceID() > mat.gameObject.GetInstanceID()))
        {
            FX_CollisionHandler.instance?.HandleCollision(this, mat, collision);
        }
    }
}
