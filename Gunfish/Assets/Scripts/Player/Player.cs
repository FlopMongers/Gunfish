using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDeviceController, IGunfishController, IUIController {
    public static int playerCount = 0;
    public int playerNumber;

    public GunfishData gunfishData;
    private Gunfish gunfish;
    private Gun gun;
    public Gunfish Gunfish { get { return gunfish; } }
    private PlayerInput input;

    public PlayerGameEvent OnDeath;

    public bool FreezeControls;
    public bool Active;

    private void OnEnable() {

    }

    private void Start() {
        DontDestroyOnLoad(gameObject);

        input = GetComponent<PlayerInput>();
        gunfish = GetComponent<Gunfish>();
        gun = GetComponent<Gun>();

        playerNumber = ++playerCount;

        gameObject.name = $"Player{playerNumber}";

        input.defaultActionMap = "UI";
    }

    public void SpawnGunfish(Vector3 spawnPosition) {
        var layer = LayerMask.NameToLayer($"Player{playerNumber}");
        gunfish.Spawn(gunfishData, layer, spawnPosition);
        input.defaultActionMap = "Player";
    }

    public void DespawnGunfish() {
        if (gunfish == null) {
            throw new UnityException($"Cannot delete Gunfish for {name} since one has not been spawned.");
        }

        gunfish.Despawn(false);
        input.defaultActionMap = "UI";
    }

    public void OnDeviceLost(PlayerInput input) {

    }

    public void OnDeviceRegained(PlayerInput input) {

    }

    public void OnControlsChanged(PlayerInput input) {

    }

    public void OnMove(InputValue value) {
        if (gunfish == null) {
            return;
        }

        if (FreezeControls) {
            gunfish.Move(Vector2.zero);
            return;
        }

        var movement = value.Get<Vector2>();
        gunfish.Move(movement);
    }

    public void OnFire(InputValue value) {
        if (gunfish == null) {
            throw new UnityException($"Cannot fire gun for {name} as a Gunfish has not been instantiated.");
        }

        if (FreezeControls) {
            return;
        }


        gunfish.SetFiring(value.isPressed); //.Fire();
    }

    public void OnNavigate(InputValue value) {

    }

    public void OnSubmit(InputValue value) {

    }
}