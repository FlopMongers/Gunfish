using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player: MonoBehaviour, IDeviceController, IGunfishController, IUIController {
    public static int playerCount = 0;
    public int playerNumber;

    public GunfishData gunfishData;
    private Gunfish gunfish;
    public Gunfish Gunfish { get { return gunfish; } }
    private PlayerInput input;

    public PlayerGameEvent OnDeath;

    public bool FreezeControls;
    public bool Active;

    private void OnEnable() {

    }

    private void Start() {
        input = GetComponent<PlayerInput>();
        playerNumber = ++playerCount;
    }

    public void SpawnGunfish(Vector3 spawnPosition) {
        var layer = LayerMask.NameToLayer($"Player{playerNumber}");
        gunfish = Gunfish.Instantiate(gunfishData, spawnPosition, layer);
    }

    public void Despawn() {
        if (gunfish == null) {
            throw new UnityException($"Cannot delete Gunfish for {name} since one has not been spawned.");
        }

        gunfish.Despawn(false);
    }

    public void OnDeviceLost(PlayerInput input) {

    }

    public void OnDeviceRegained(PlayerInput input) {

    }

    public void OnControlsChanged(PlayerInput input) {

    }

    public void OnMove(InputValue value) {
        if (gunfish == null) {
            throw new UnityException($"Cannot move Gunfish for {name} as one has not been instantiated.");
        }
        var movement = value.Get<Vector2>();
        gunfish.Move(movement);
    }

    public void OnFire(InputValue value) {
        if (gunfish == null) {
            throw new UnityException($"Cannot fire gun for {name} as a Gunfish has not been instantiated.");
        }

        if (value.isPressed) {
            gunfish.Fire();
        }
    }
}
