using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDeviceController, IGunfishController, IUIController {
    public int PlayerNumber { get; private set; }

    public GunfishData gunfishData;
    private Gunfish gunfish;
    private Gun gun;
    public Gunfish Gunfish { get { return gunfish; } }
    private PlayerInput input;

    public PlayerGameEvent OnDeath;

    public bool FreezeControls;
    public bool Active;

    public int layer;

    private void OnEnable() {

    }

    public void Initialize(int playerNumber) {
        DontDestroyOnLoad(gameObject);
        
        PlayerNumber = playerNumber;

        input = GetComponent<PlayerInput>();
        gunfish = GetComponent<Gunfish>();
        gun = GetComponent<Gun>();

        layer = LayerMask.NameToLayer($"Player{PlayerNumber+1}");

        gameObject.name = $"Player{PlayerNumber}";

        input.defaultActionMap = "UI";
    }

    public void SpawnGunfish(Vector3 spawnPosition) {
        var color = PlayerManager.Instance.playerColors[PlayerNumber];
        gunfish.Spawn(gunfishData, layer, spawnPosition);
        var material = gunfish.renderer.LineRenderer.material;
        material.SetColor("_OutlineColor", color);
        material.SetFloat("_OutlineWidth", 0.01f);
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