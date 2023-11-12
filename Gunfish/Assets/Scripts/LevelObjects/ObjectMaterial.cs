using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMaterial : MonoBehaviour {
    public bool skipCollision;
    public MaterialType materialType;

    protected void HandleMaterialCollision(ObjectMaterial mat, Collision2D collision) {
        if (mat == null) {
            FX_CollisionHandler.Instance?.HandleDefaultCollision(this, collision);
            return;
        }
        // if other skip collision, send the collision to the fx handler
        if (mat.skipCollision == true || materialType > mat.materialType || (materialType == mat.materialType && GetInstanceID() > mat.gameObject.GetInstanceID())) {
            FX_CollisionHandler.Instance?.HandleCollision(this, mat, collision);
        }
    }

    // on collision, if not external collision and not ground
    public virtual void OnCollisionEnter2D(Collision2D collision) {
        if (skipCollision)
            return;

        // if other has no thingy, play send to handler with default
        HandleMaterialCollision(collision.collider.GetComponent<ObjectMaterial>(), collision);

    }
}