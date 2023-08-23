using UnityEngine.UIElements.Experimental;

public class GunfishStatusData {

    private bool canMove = true;
    public bool CanMove { get => alive && canMove; set => canMove = value; }

    private bool canFire=true;
    public bool CanFire { get => CanMove && canFire == true; set => canFire = value; }

    public float flopTimer = 0f;
    public float flopForce;
    public bool CanFlop { get => flopTimer < UnityEngine.Mathf.Epsilon && CanMove; }
    public bool inputLocked = false;

    public float health;
    public bool alive { get => health > 0f; }
}
