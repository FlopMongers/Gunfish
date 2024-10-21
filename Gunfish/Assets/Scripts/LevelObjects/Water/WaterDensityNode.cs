using UnityEngine;

public class WaterDensityNode : MonoBehaviour {
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private BoxCollider2D col;
    [SerializeField]
    private BuoyancyEffector2D effector;

    public WaterSurfaceNode left;
    public WaterSurfaceNode right;

    private bool initialized = false;

    public void Init(WaterSurfaceNode left, WaterSurfaceNode right, Vector2 size) {
        this.left = left;
        this.right = right;
        col.size = size;
        col.offset = new Vector2(-size.x / 2f, 0f);
        initialized = true;
    }

    public void Update() {
        if (!initialized) return;
    }
}
