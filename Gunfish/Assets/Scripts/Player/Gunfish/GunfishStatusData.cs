public class GunfishStatusData {
    public float stunTimer = 0f;
    public bool IsStunned { get => stunTimer > 0f; }

    public float reloadTimer = 0f;
    public bool CanFire { get => reloadTimer < UnityEngine.Mathf.Epsilon; }

    public float flopTimer = 0f;
    public bool CanFlop { get => flopTimer < UnityEngine.Mathf.Epsilon; }
    public bool inputLocked = false;
}
