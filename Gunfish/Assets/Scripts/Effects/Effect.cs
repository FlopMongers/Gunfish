using System;
using System.Drawing.Printing;
using UnityEngine;


public enum EffectType { FlopModify, NoMove, Underwater, SharkMode, Zap, Invincibility };


[Serializable]
public class Effect {
    public Gunfish gunfish;
    public EffectType effectType;

    public Effect(Gunfish gunfish) {
        this.gunfish = gunfish;
    }

    public virtual void Check(Effect effect) {
        // possibly respond to the addition of another effect
    }

    public virtual void OnAdd() {
        // do something to that gunfish
    }

    public virtual void Update() {
        // keep doing something to that gunfish 
    }

    public virtual void Merge(Effect effect) {
        // eat that friggin effect
    }

    public virtual void OnRemove() {
        // stop doing what you were doing to that poor gunfish
    }
}

[Serializable]
public class FlopModify_Effect : Effect {
    public float flopMultiplier;

    public FlopModify_Effect(Gunfish gunfish, float flopMultiplier) : base(gunfish) {
        this.flopMultiplier = flopMultiplier;
        effectType = EffectType.FlopModify;
    }

    public override void OnAdd() {
        base.OnAdd();
        gunfish.statusData.flopForce *= flopMultiplier;
    }

    public override void Merge(Effect effect) {
        FlopModify_Effect t_effect = (FlopModify_Effect)effect;
        flopMultiplier += t_effect.flopMultiplier;
    }

    public override void Update() {
        base.Update();
        if (Mathf.Approximately(flopMultiplier, 0)) {
            gunfish.RemoveEffect(effectType);
        }
    }

    public override void OnRemove() {
        base.OnRemove();
        gunfish.statusData.flopForce = gunfish.data.flopForce;
    }
}

// consider using composition over inheritance here
[Serializable]
public class TimedEffect : Effect {
    public float timer;

    public TimedEffect(Gunfish gunfish, float timer) : base(gunfish) {
        this.timer = timer;
    }

    public override void Merge(Effect effect) {
        base.Merge(effect);
        timer = Mathf.Max(((TimedEffect)effect).timer, timer);
    }

    public override void Update() {
        base.Update();
        timer -= Time.deltaTime;
        if (timer <= 0) {
            gunfish.RemoveEffect(effectType);
        }
    }
}

[Serializable]
public class Zap_Effect : TimedEffect {

    enum MovementType { MoveLeft, MoveRight };
    static int MovementTypeCount = 3;

    MovementType affectedMovementType;
    public static Vector2 delayRange = new Vector2(0.1f, 0.3f);
    public static Vector2 zapDurationRange = new Vector2(0.5f, 1f);
    float stateTimer;
    bool zappin;

    public Zap_Effect(Gunfish gunfish, float timer) : base(gunfish, timer) {
        this.effectType = EffectType.Zap;
    }

    public override void OnAdd() {
        base.OnAdd();
        gunfish.AddEffect(new NoMove_Effect(gunfish));
        StartZappin();
    }

    public override void Update() {
        base.Update();
        // randomly move or shoot
        stateTimer -= Time.deltaTime;
        if (zappin) {
            switch (affectedMovementType) {
                case MovementType.MoveLeft:
                    gunfish.Move(Vector2.left);
                    break;
                case MovementType.MoveRight:
                    gunfish.Move(Vector2.right);
                    break;
            }
            gunfish.Movement(true);
        }
        if (stateTimer <= 0) {
            if (zappin) {
                zappin = false;
                stateTimer = delayRange.RandomInRange();
            }
            else {
                StartZappin();
            }
        }

    }
    void StartZappin() {
        affectedMovementType = (MovementType)UnityEngine.Random.Range(0, MovementTypeCount);
        stateTimer = zapDurationRange.RandomInRange();
        zappin = true;
    }

    public override void OnRemove() {
        base.OnRemove();
        gunfish.RemoveEffect(EffectType.NoMove);
    }
}

// invincibility effect
// receive fish hit and nullify it
// todo: invincibility fx object
[Serializable]
public class Invincibility_Effect : TimedEffect {

    GameObject fx;

    public Invincibility_Effect(Gunfish gunfish, float timer) : base(gunfish, timer) {
        effectType = EffectType.Invincibility;
    }

    public override void OnAdd() {
        base.OnAdd();
        // subscribe to gunfish composite collision detection
        gunfish.OnHit += OnHit;
        // todo: spawn sharkmode music
        fx = FX_Spawner.Instance.SpawnFX(FXType.Invincibility, gunfish.RootSegment.transform.position, Quaternion.identity, parent: gunfish.RootSegment.transform);
    }

    public override void OnRemove() {
        base.OnRemove();
        // unsubscribe from composiite collision detection event
        gunfish.OnHit -= OnHit;
        // stop sharkmode music
        // NOTE(Wyatt): stupid workaround.
        // todo: fade out effect instead of just DELET
        FX_Spawner.Instance.DestroyFX(fx);
    }

    void OnHit(Gunfish gunfish, FishHitObject hit) {
        // eliminate damage?
        if (hit.source != null && hit.source.GetComponent<KillBox>()) {
            return;
        }
        hit.damage = 0;
    }
}

// NOTE(Wyatt): currently, this is the only effect that modifies these values.
// In the future, I'm considering changing stats like underWaterForce and flopForce and all that to special objects that track multiplicative and additive modifiers
[Serializable]
public class Sharkmode_Effect : TimedEffect {

    public static float underwaterForceModifier = 2f;
    GameObject fx;

    public Sharkmode_Effect(Gunfish gunfish, float timer) : base(gunfish, timer) {
        effectType = EffectType.SharkMode;
    }

    public override void OnAdd() {
        base.OnAdd();
        // update underwaterForce and maxUnderwaterForce
        gunfish.statusData.underwaterForceMultiplier += underwaterForceModifier;
        gunfish.statusData.maxUnderwaterVelocityMultiplier += underwaterForceModifier;
        // subscribe to gunfish composite collision detection
        gunfish.RootSegment.GetComponent<CompositeCollisionDetector>().OnComponentCollideEnter += OnCollision;
        // todo: spawn sharkmode music
        if (FX_Spawner.Instance != null) {
            fx = FX_Spawner.Instance.SpawnFX(FXType.SharkMode, gunfish.RootSegment.transform.position, Quaternion.identity, parent: gunfish.RootSegment.transform);
        }
        if (SharkmodeManager.Instance != null) {
            Debug.Log("sharkmode >:)");
            SharkmodeManager.Instance.UpdateCounter(gunfish, true);
        }
    }

    public override void OnRemove() {
        base.OnRemove();
        // reset underwaterForce and maxUnderwaterForce
        gunfish.statusData.underwaterForceMultiplier -= underwaterForceModifier;
        gunfish.statusData.maxUnderwaterVelocityMultiplier -= underwaterForceModifier;
        // unsubscribe from composiite collision detection event
        gunfish.RootSegment.GetComponent<CompositeCollisionDetector>().OnComponentCollideEnter -= OnCollision;
        // stop sharkmode music
        // NOTE(Wyatt): stupid workaround.
        // todo: fade out effect instead of just DELET
        if (SharkmodeManager.Instance != null) {
            Debug.Log("No more sharkmode :(");
            SharkmodeManager.Instance.UpdateCounter(gunfish, false);
        }
        FX_Spawner.Instance.DestroyFX(fx);
    }

    public void OnCollision(GameObject src, Collision2D collision) {
        // if it's a fish and it's not in sharkmode, fucking KILL IT!
        GunfishSegment segment = collision.collider.GetComponent<GunfishSegment>();
        if (segment == null || segment.gunfish == gunfish)
            return;
        if (!segment.gunfish.effectMap.ContainsKey(EffectType.SharkMode)) {
            segment.gunfish.Hit(new FishHitObject(segment.index, collision.contacts[0].point, -collision.contacts[0].normal, gunfish.gameObject, segment.gunfish.statusData.health, 10f, HitType.Impact));
        }
    }
}

// NOTE: once again, consider composition over inheritance
public class CounterEffect : Effect {
    public int counter;

    public CounterEffect(Gunfish gunfish, int counter) : base(gunfish) {
        this.counter = counter;
    }

    public override void Merge(Effect effect) {
        // eat that friggin effect
        CounterEffect t_effect = (CounterEffect)effect;
        counter += t_effect.counter;
    }

    public override void Update() {
        base.Update();
        if (counter <= 0)
            gunfish.RemoveEffect(effectType);
    }
}

[Serializable]
public class NoMove_Effect : CounterEffect {

    public NoMove_Effect(Gunfish gunfish, int counter = 1) : base(gunfish, counter) {
        effectType = EffectType.NoMove;
    }

    public override void OnAdd() {
        base.OnAdd();
        gunfish.statusData.CanMove = false;
    }

    public override void OnRemove() {
        base.OnRemove();
        gunfish.statusData.CanMove = true;
    }
}