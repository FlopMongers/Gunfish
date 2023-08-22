public class GunfishStatusData {
    public float stunTimer = 0f;
    public bool IsStunned { get => stunTimer > 0f; }

    public bool canFire = true;
    public bool CanFire { get => alive && canFire; }

    public float flopTimer = 0f;
    public float flopForce;
    public bool CanFlop { get => flopTimer < UnityEngine.Mathf.Epsilon && alive; }
    public bool inputLocked = false;

    public float health;
    public bool alive { get => health > 0f; }
}
