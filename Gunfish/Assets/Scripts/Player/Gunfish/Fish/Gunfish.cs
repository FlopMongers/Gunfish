using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ButtonStatus { Pressed, Holding, Released, Up };
public class Gunfish : MonoBehaviour, IHittable {
    public Dictionary<EffectType, Effect> effectMap = new Dictionary<EffectType, Effect>();
    
    [HideInInspector]
    public List<EffectType> EffectRemoveList = new List<EffectType>();

    public GunfishStatusData statusData;
    public GunfishData data;
    public bool debug = false;

    [HideInInspector]
    public List<GameObject> segments;
    public int MiddleSegmentIndex { get { return segments.Count / 2; } }
    public GameObject MiddleSegment { get { return (segments.Count > 0) ? segments[MiddleSegmentIndex] : null; } }
    public GameObject RootSegment { get { return (segments.Count > 0) ? segments[0]: null; } }
    private GunfishGenerator generator;
    [HideInInspector]
    public GunfishRenderer gunfishRenderer;
    [HideInInspector]
    public GunfishRigidbody body;
    GroundDetector groundDetector;
    [HideInInspector]
    public Gun gun;

    Destroyer destroyer;

    [HideInInspector]
    public Player player;
    public PlayerGameEvent OnDeath;
    public FloatGameEvent OnHealthUpdated;
    public FishHitEvent OnHit;
    private bool killed;
    private bool spawned;

    private Vector2 movement;

    public int playerNum;

    [HideInInspector]
    public bool underwater;

    [HideInInspector]
    public int anySegmentUnderwater;

    ButtonStatus firingStatus = ButtonStatus.Up;

    float respawnHoldDuration = 3f;
    float respawnHoldTimer;
    bool startRespawning = false;
    public FloatGameEvent OnRespawnUpdated;

    static float spawnInvincibilityDuration = 2f;

    private void Start() {
        player = GetComponent<Player>();
        playerNum = player.PlayerNumber;

        killed = false;
        spawned = false;
        
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindActionMap("EndLevel").FindAction("Submit").performed += ctx => { GameModeManager.Instance?.NextLevel(); };
    }

    private void Update() {

        if (killed || !spawned) {
            return;
        }

        if (startRespawning == true && player.FreezeControls == false) {
            respawnHoldTimer += Time.deltaTime;
        }
        else {
            respawnHoldTimer = 0f;
        }
        OnRespawnUpdated?.Invoke(respawnHoldTimer / respawnHoldDuration);
        if (respawnHoldTimer >= respawnHoldDuration) {
            Hit(new FishHitObject(MiddleSegmentIndex, MiddleSegment.transform.position, Vector2.zero, gameObject, statusData.health, 0, HitType.Explosive));
        }

        HandleEffects();

        gunfishRenderer?.Render();
        DecrementTimers(Time.deltaTime);
    }

    private void FixedUpdate() {
        CheckFiring();
        Fire();
        Movement();
    }

    void HandleEffects() {
        if (!statusData.alive) {
            foreach (var effect in effectMap) {
                EffectRemoveList.Add(effect.Key);
            }
            foreach (var effect in EffectRemoveList) {
                if (effectMap.ContainsKey(effect)) {
                    if (effectMap[effect] != null) {
                        effectMap[effect].OnRemove();
                    }
                    effectMap.Remove(effect);
                }
            }
            EffectRemoveList.Clear();
            FX_Spawner.Instance?.SpawnFX(FXType.Fish_Death, MiddleSegment.transform.position, Quaternion.identity);
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
        while (EffectRemoveList.Count > 0) {
            var effect = EffectRemoveList[0];
            if (effectMap.ContainsKey(effect)) {
                effectMap[effect].OnRemove();
                effectMap.Remove(effect);
            }
            EffectRemoveList.RemoveAt(0);
        }
        EffectRemoveList.Clear();
    }

    public void AddEffect(Effect effect) {
        foreach (var srcEffect in effectMap.Values) {
            srcEffect.Check(effect);
        }

        if (effectMap.ContainsKey(effect.effectType)) {
            effectMap[effect.effectType].Merge(effect);
        }
        else {
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

    public void Movement(bool forceMove=false) {
        if (statusData == null || RootSegment == null || (!statusData.CanMove && !forceMove))
            return;

        // if underwater
        if (movement.sqrMagnitude > Mathf.Epsilon) {
            if (groundDetector != null && groundDetector.IsGrounded()) {
                if (statusData.CanFlop) {
                    GroundedMovement(movement);
                }
            }
            else if (underwater) {
                Swim();
            } else if (Mathf.Abs(movement.x) >= 0.2f) {
                RotateMovement(movement, 0, data.airTorque);
            }
        }
    }

    private void GroundedMovement(Vector2 input) {
        // reset flop timer
        statusData.flopTimer = data.flopCooldown;
        var index = segments.Count / 2; //movement.x > 0f ? Mathf.RoundToInt(segments.Count * 0.25f) : Mathf.RoundToInt(segments.Count * 0.75f);
        var direction = movement.x > 0f ? new Vector2(1f, 1f).normalized : new Vector2(-1f, 1f).normalized;
        // flop force
        body.ApplyForceToSegment(index, direction * data.flopForce, ForceMode2D.Impulse);
        RotateMovement(input, index, data.groundTorque, ForceMode2D.Impulse);
        // play flop
        // TODO play correct flop sound depending on material
        FX_Spawner.Instance?.SpawnFX(FXType.Flop, segments[index].transform.position, Quaternion.identity);
    }

    private void RotateMovement(Vector2 input, int segmentIndex, float torque, ForceMode2D forceMode = ForceMode2D.Force) {
        var direction = Mathf.Sign(input.x);
        // rotation speed
        if (Mathf.Sign(-direction) != Mathf.Sign(body.segments[segmentIndex].body.angularVelocity) || Mathf.Abs(body.segments[segmentIndex].body.angularVelocity) < data.maxAerialAngularVelocity)
            body.ApplyTorqueToSegment(segmentIndex, -direction * torque, forceMode);
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
        if (!statusData.CanMove) {
            return;
        }

        if (body.segments[0].body.velocity.magnitude < data.maxUnderwaterVelocity) {
            body.ApplyForceToSegment(0, movement * data.underwaterForce, ForceMode2D.Force);
        }
    }

    public void Fire() {
        if (statusData == null || statusData.alive == false)
            return;
        // NOTE(Wyatt): just uncomment if you don't want to be able to shoot underwater
        // if underwater, then zoom
        /*if (underwater == true) {
            Swim();
        }*/
        //else {
            gun?.Fire(firingStatus);
        //}
    }

    public void SetRespawn(bool respawn) {
        // NOTE(Wyatt): I intensely dislike how incongruent unity's new input system is from the rest of the engine
        print(startRespawning);
        startRespawning = respawn;
    }

    public void Hit(FishHitObject hit) {
        // TODO tell match manager about this for possible scoring
        // TODO: replace with generalized FX_CollisionHandler?
        bool alreadyDead = statusData.health <= 0;
        OnHit?.Invoke(this, hit);
        if (hit.damage > 0 && hit.ignoreFX == false)
            FX_Spawner.Instance?.SpawnFX(FXType.Fish_Hit, hit.position, -hit.direction);
        if (hit.ignoreMass) {
            hit.knockback *= data.mass;
        }
        body.ApplyForceToSegment(hit.segmentIndex, hit.direction * hit.knockback, ForceMode2D.Impulse);
        UpdateHealth(-hit.damage);
        GameModeManager.Instance.matchManagerInstance.HandleFishDamage(hit, this, alreadyDead);
    }

    public void UpdateHealth(float amount) {
        statusData.health = Mathf.Clamp(statusData.health + amount, 0, data.maxHealth);
        OnHealthUpdated?.Invoke(statusData.health);
    }

    public Vector3? GetPosition() {
        if (!killed && spawned)
            return segments[0].transform.position;
        else
            return null;
    }

    public Bounds GetBoundingBox() {
        List<Collider2D> colliders = new(10);
        segments[0].GetComponent<Rigidbody2D>().GetAttachedColliders(colliders);
        Bounds bounds = colliders[0].bounds;
        bounds = AddCollidersToBounds(bounds, colliders);
        for (int i = 1; i < segments.Count; i++) {
            colliders.Clear();
            segments[i].GetComponent<Rigidbody2D>().GetAttachedColliders(colliders);
            bounds = AddCollidersToBounds(bounds, colliders);
        }
        return bounds;
    }

    private Bounds AddCollidersToBounds(Bounds bounds, List<Collider2D> colliders) {
        foreach (Collider2D collider in colliders) {
            bounds.Encapsulate(collider.bounds.min);
            bounds.Encapsulate(collider.bounds.max);
        }
        return bounds;
    }

    public void Spawn(GunfishData data, LayerMask layer, Vector3 position) {
        if (data.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {data.segmentCount}. Must be greater than or equal to 3.");
        }
        SetFiring(false);
        this.underwater = false;
        startRespawning = false;
        this.data = data;
        gun = Instantiate(data.gun.gunPrefab, transform).GetComponent<Gun>();
        gun.gunfish = this;
        gun.ammo = data.gun.maxAmmo;

        this.statusData = new GunfishStatusData();
        statusData.health = data.maxHealth;
        statusData.flopForce = data.flopForce;

        generator = new GunfishGenerator(this);

        segments = generator.Generate(layer, position);

        if (FX_Spawner.Instance != null) {
            // TODO, init properly
            var healthUI = Instantiate(FX_Spawner.Instance.fishHealthUIPrefab).GetComponent<HealthUI>();
            healthUI.Init(this);
            FX_Spawner.Instance.SpawnFX(FXType.Spawn, MiddleSegment.transform.position, Quaternion.identity);
        }

        // width in sprite mat units means the width in pixels - i.e. tail-to-tip of fish
        // whereas width here refers to line renderer width - i.e. back-to-belly of fish
        float width = (
            (float)data.spriteMat.mainTexture.height / (float)data.spriteMat.mainTexture.width
        ) * data.length;
        gunfishRenderer = new GunfishRenderer(width, data.spriteMat, segments);
        body = new GunfishRigidbody(segments);


        //RootSegment.AddComponent<LineFader>();
        destroyer = RootSegment.AddComponent<Destroyer>();
        // add composite detection handler and Init
        // add damage receiver
        CollisionDamageReceiver receiver = RootSegment.CheckAddComponent<CollisionDamageReceiver>();
        receiver.oomphScale = 0.5f;
        receiver.gunfish = this;
        RootSegment.CheckAddComponent<CompositeCollisionDetector>().Init(true, true, true);
        groundDetector = RootSegment.CheckAddComponent<GroundDetector>();
        groundDetector.gunfish = this;
        groundDetector.groundMask = LayerMask.GetMask("Ground", "Player1", "Player2", "Player3", "Player4", "Default") & ~(1 << layer);

        spawned = true;
        killed = false;

        var gunSprite = Instantiate(
            data.gun.gunSpritePrefab,
            RootSegment.transform
        ).transform;
        gunSprite.transform.localPosition = new Vector3(
            data.gunOffset.position.x,
            data.gunOffset.position.y
        );
        gunSprite.gameObject.layer = layer;
        foreach (Transform child in gunSprite) {
            child.gameObject.layer = layer;
        }

        float gun_length = gunSprite.gameObject.GetComponentInChildren<SpriteRenderer>().sprite.texture.width;
        float desired_world_length = gun_length * (data.length / data.spriteMat.mainTexture.width);
        float current_world_length = gunSprite.gameObject.GetComponentInChildren<SpriteRenderer>().sprite.bounds.size.x;
        gunSprite.localScale *= desired_world_length / current_world_length;

        foreach (TransformTuple tuple in data.gun.gunBarrels) {
            // spawn
            var barrel = Instantiate(data.gun.gunBarrelPrefab).transform; // new GameObject("barrel").transform;
            barrel.parent = segments[0].transform;
            barrel.localPosition = tuple.position;
            barrel.localEulerAngles = Vector3.forward * tuple.rotation;
            gun.barrels.Add(barrel.gameObject.GetComponent<GunBarrel>());
        }
        AddEffect(new Invincibility_Effect(this, spawnInvincibilityDuration));
    }

    public void Kill() { statusData.health = 0f; }

    public void Despawn(bool animated) {
        // if animated, then fade and destroy
        Destroy(gun.gameObject);
        DespawnSegments(animated);
    }

    private void DespawnSegments(bool animated) {
        while (segments.Count > 0) {
            var segment = segments[0];
            segments.Remove(segment);
            if (!animated)
                Destroy(segment);
            //DespawnSegment(segment, animated);
        }
        if (animated)
            destroyer.GETTEM();
    }

    public void Garbulate() {
        if (data == null) {
            Debug.LogError("Cannot garbulate Gunfish. Please ensure the Gunfish Data field is populated.");
            return;
        }
    }
}
