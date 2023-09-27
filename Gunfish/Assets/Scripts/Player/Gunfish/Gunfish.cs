using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Reflection;

public enum ButtonStatus { Pressed, Holding, Released, Up };
public class Gunfish : MonoBehaviour {
    // public static Gunfish Instantiate(GunfishData data, Vector3 position, Player player, LayerMask layer) {
    //     var instance = new GameObject($"{player.name}GunfishHandler");
    //     instance.transform.SetPositionAndRotation(position, Quaternion.identity);
    //     var gunfish = instance.AddComponent<Gunfish>();
    //     gunfish.player = player;
    //     gunfish.Spawn(data, layer);
    //     return gunfish;
    // }

    public Dictionary<EffectType, Effect> effectMap = new Dictionary<EffectType, Effect>();
    [HideInInspector]
    public List<EffectType> EffectRemoveList = new List<EffectType>();

    public GunfishStatusData statusData;
    public GunfishData data;
    public bool debug = false;

    [HideInInspector]
    public List<GameObject> segments;
    public int MiddleSegmentIndex { get { return segments.Count / 2; } }
    public GameObject MiddleSegment { get { return segments[MiddleSegmentIndex]; } }
    private GunfishGenerator generator;
    private new GunfishRenderer renderer;
    [HideInInspector]
    public GunfishRigidbody body;
    [HideInInspector]
    public Gun gun;

    private InputActionMap inputHandler;

    private Player player;
    public PlayerGameEvent OnDeath;
    public FloatGameEvent OnHealthUpdated;
    private bool killed;
    private bool spawned;

    private Vector2 movement;

    public int playerNum;

    [HideInInspector]
    public bool underwater;

    ButtonStatus firingStatus = ButtonStatus.Up;

    private void Start() {
        gun = GetComponent<Gun>();
        gun.gunfish = this;

        player = GetComponent<Player>();

        killed = false;
        spawned = false;

        // Marquee manager does not currently care about player. Encapsulating in anonymous delegate for now.
        OnDeath += (Player player) => { MarqueeManager.instance.EnqueueRandomQuip(); };

        PlayerInput playerInput = GetComponent<PlayerInput>();
        inputHandler = playerInput.actions.FindActionMap("Player");
        //inputHandler.FindAction("Fire").performed += ctx => { gun?.Fire(); };
        playerInput.actions.FindActionMap("EndLevel").FindAction("Submit").performed += ctx => { MatchManager.instance?.NextLevel(); };
    }

    private void OnDestroy() {
        OnDeath -= (Player player) => { MarqueeManager.instance.EnqueueRandomQuip(); };
    }

    private void Update() {

        if (killed || !spawned) {
            return;
        }

        HandleEffects();

        renderer?.Render();
        DecrementTimers(Time.deltaTime);
    }

    private void FixedUpdate() {
        CheckFiring();
        Fire();
        Movement();
    }

    void HandleEffects() {
        if (!statusData.alive) {
            // kill da fish
            foreach (var effect in effectMap)
                EffectRemoveList.Add(effect.Key);
            foreach (var effect in EffectRemoveList)
                effectMap.Remove(effect);
            EffectRemoveList.Clear();
            FX_Spawner.instance?.SpawnFX(FXType.Fish_Death, MiddleSegment.transform.position, Quaternion.identity);
            Despawn(true);
            killed = true;
            OnDeath?.Invoke(player);
            return;
        }

        if (segments.Count > 0 && segments[0] == null) {
            Despawn(false);
            spawned = false;
            return;
        }

        foreach (var effect in effectMap.Values) {
            effect.Update();
        }
        foreach (var effect in EffectRemoveList) {
            if (effectMap.ContainsKey(effect)) {
                effectMap[effect].OnRemove();
                effectMap.Remove(effect);
            }
        }
        EffectRemoveList.Clear();
    }

    public void AddEffect(Effect effect) {
        foreach (var srcEffect in effectMap.Values) 
        {
            srcEffect.Check(effect);
        }

        if (effectMap.ContainsKey(effect.effectType)) {
            effectMap[effect.effectType].Merge(effect);
        } else {
            effectMap[effect.effectType] = effect;
            effect.OnAdd();
        }
    }

    public void RemoveEffect(EffectType effectType) {
        EffectRemoveList.Add(effectType);
    }

    private void DecrementTimers(float delta) {
        statusData.flopTimer = Mathf.Max(0f, statusData.flopTimer - delta);
    }

    private void Movement() {
        if (statusData == null || !statusData.CanMove) return;

        // if underwater

        if (movement.sqrMagnitude > Mathf.Epsilon) {
            if (body.Grounded) {
                if (statusData.CanFlop)
                    GroundedMovement(movement);
            } else if (underwater) {
                RotateMovement(movement, data.underwaterTorque);
            }
            RotateMovement(movement);
        }
    }

    private void GroundedMovement(Vector2 input) {
        // reset flop timer
        statusData.flopTimer = data.flopCooldown;
        var index = segments.Count / 2; //movement.x > 0f ? Mathf.RoundToInt(segments.Count * 0.25f) : Mathf.RoundToInt(segments.Count * 0.75f);
        var direction = movement.x > 0f ? new Vector2(1f, 1f).normalized : new Vector2(-1f, 1f).normalized;
        // flop force
        body.ApplyForceToSegment(index, direction * data.flopForce, ForceMode2D.Impulse);
        RotateMovement(input, data.groundTorque, ForceMode2D.Impulse);
        // play flop
        FX_Spawner.instance?.SpawnFX(FXType.Flop, segments[index].transform.position, Quaternion.identity);
    }

    private void RotateMovement(Vector2 input, float? airTorque=null, ForceMode2D forceMode = ForceMode2D.Force) {
        var index = segments.Count / 2;
        var direction = Mathf.Sign(input.x);
        // rotation speed
        if (Mathf.Sign(-direction) != Mathf.Sign(body.segments[index].body.angularVelocity) || Mathf.Abs(body.segments[index].body.angularVelocity) < data.maxAerialAngularVelocity)
            body.ApplyTorqueToSegment(index, -direction * airTorque.GetValueOrDefault(data.airTorque), forceMode);
    }

    public void Move(Vector2 movement) {
        this.movement = movement;
    }


    bool fireInteracted, firePressed;

    public void SetFiring(bool firing) {
        fireInteracted = true;
        firePressed = firing;
    }

    void CheckFiring() {
        // pressed to holding
        if (firingStatus == ButtonStatus.Pressed && firePressed) {
            firingStatus = ButtonStatus.Holding;
            return;
        }
        if (firingStatus == ButtonStatus.Released && !firePressed) {
            firingStatus = ButtonStatus.Up;
            return;
        }
        if (!fireInteracted)
            return;
        fireInteracted = false;
        if (firingStatus == ButtonStatus.Up || firingStatus == ButtonStatus.Released) {
            if (firePressed) {
                firingStatus = ButtonStatus.Pressed;
                return;
            }
        }
        if (firingStatus == ButtonStatus.Pressed) {
            if (!firePressed) {
                firingStatus = ButtonStatus.Released;
                return;
            }
        }
        if (firingStatus == ButtonStatus.Holding) {
            if (!firePressed) {
                firingStatus = ButtonStatus.Released;
            }
        }
    }
    void Swim() {
        // TODO handle surge swimming logic
        // for now, if pressed or holding, then swim
        if (!statusData.CanMove || (firingStatus != ButtonStatus.Pressed && firingStatus != ButtonStatus.Holding))
            return;
        int index = segments.Count / 2;
        if (Vector3.Project(body.segments[index].body.velocity, segments[index].transform.right).magnitude < data.maxUnderwaterVelocity) {
            body.ApplyForceToSegment(index, -segments[index].transform.right * data.underwaterForce, ForceMode2D.Force);
        }
    }

    public void Fire() {
        if (statusData == null || statusData.alive == false)
            return;
        // if underwater, then zoom
        if (underwater == true) {
            Swim();
        } else {
            gun.Fire(firingStatus);
        }
    }

    public void Hit(FishHitObject hit) {
        // tell match manager about this for possible scoring
        // TODO: replace with generalized FX_CollisionHandler?
        FX_Spawner.instance?.SpawnFX(FXType.Fish_Hit, hit.position, -hit.direction);
        body.ApplyForceToSegment(hit.segmentIndex, hit.direction * hit.knockback, ForceMode2D.Impulse);
        UpdateHealth(-hit.damage);
    }

    public void UpdateHealth(float amount) {
        statusData.health += amount;
        OnHealthUpdated?.Invoke(statusData.health);
    }

    public Vector3? GetPosition() {
        if (!killed && spawned) return segments[0].transform.position;
        else return null;
    }

    public void Spawn(GunfishData data, LayerMask layer, Vector3 position) {
        if (data.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {data.segmentCount}. Must be greater than or equal to 3.");
        }
        this.data = data;
        gun.ammo = data.gun.maxAmmo;

        this.statusData = new GunfishStatusData();
        statusData.health = data.maxHealth;
        statusData.flopForce = data.flopForce;

        generator = new GunfishGenerator(this);

        segments = generator.Generate(layer, position);

        if (FX_Spawner.instance != null)
        {
            // TODO, init properly
            var healthUI = Instantiate(FX_Spawner.instance.fishHealthUIPrefab).GetComponent<HealthUI>();
            healthUI.Init(this);
            FX_Spawner.instance.SpawnFX(FXType.Spawn, MiddleSegment.transform.position, Quaternion.identity);
        }

        renderer = new GunfishRenderer(data.spriteMat, segments);
        body = new GunfishRigidbody(segments, layer);
        // add composite detection handler and Init
        // add damage receiver
        segments[0].CheckAddComponent<CollisionDamageReceiver>().gunfish = this;
        segments[0].CheckAddComponent<CompositeCollisionDetector>().Init(true, true, true);

        spawned = true;
        killed = false;

        foreach (TransformTuple tuple in data.gun.gunBarrels) {
            // spawn
            var barrel = Instantiate(data.gun.gunBarrelPrefab).transform; // new GameObject("barrel").transform;
            barrel.parent = segments[0].transform;
            barrel.localPosition = tuple.position;
            barrel.localEulerAngles = Vector3.forward * tuple.rotation;
            gun.barrels.Add(barrel.gameObject.GetComponent<GunBarrel>());
        }
    }

    public void Kill() { statusData.health = 0f; }

    public void Despawn(bool animated) {
        gun.barrels = new List<GunBarrel>();
        DespawnSegments(animated);
    }

    private void DespawnSegments(bool animated) {
        while (segments.Count > 0) {
            var segment = segments[0];
            segments.Remove(segment);
            DespawnSegment(segment, animated);
        }
    }

    private void DespawnSegment(GameObject segment, bool animated) {
        if (animated) {
            // Todo: replace with cool animated fish guts or something
            Destroy(segment);
        } else {
            Destroy(segment);
        }
    }
}
