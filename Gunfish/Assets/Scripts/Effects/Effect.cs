using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

public enum EffectType { FlopModify, NoMove, Underwater, SharkMode };


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

// NOTE(Wyatt): currently, this is the only effect that modifies these values.
// In the future, I'm considering changing stats like underWaterForce and flopForce and all that to special objects that track multiplicative and additive modifiers
[Serializable]
public class Sharkmode_Effect : Effect {

    public float timer;

    public static float underwaterForceModifier = 2f;

    public Sharkmode_Effect(Gunfish gunfish, float timer) : base(gunfish) {
        this.timer = timer;
    }

    public override void OnAdd() {
        base.OnAdd();
        // update underwaterForce and maxUnderwaterForce
        gunfish.statusData.underwaterForceMultiplier += underwaterForceModifier;
        gunfish.statusData.maxUnderwaterVelocityMultiplier += underwaterForceModifier;
        // subscribe to gunfish composite collision detection
        gunfish.RootSegment.GetComponent<CompositeCollisionDetector>().OnComponentCollideEnter += OnCollision;
        // todo: spawn sharkmode music
    }

    public override void Merge(Effect effect) {
        base.Merge(effect);
        timer += ((Sharkmode_Effect)effect).timer;
    }

    public override void OnRemove() {
        base.OnRemove();
        // reset underwaterForce and maxUnderwaterForce
        gunfish.statusData.underwaterForceMultiplier -= underwaterForceModifier;
        gunfish.statusData.maxUnderwaterVelocityMultiplier -= underwaterForceModifier;
        // unsubscribe from composiite collision detection event
        gunfish.RootSegment.GetComponent<CompositeCollisionDetector>().OnComponentCollideEnter -= OnCollision;
        // stop sharkmode music
    }

    public override void Update() {
        base.Update();
        timer -= Time.deltaTime;
        if (timer <= 0) {
            gunfish.RemoveEffect(effectType);
        }
    }

    public void OnCollision(GameObject src, Collision2D collision) {
        // if it's a fish and it's not in sharkmode, fucking KILL IT!
        GunfishSegment segment = src.GetComponent<GunfishSegment>();
        if (segment == null || segment.gunfish == gunfish)
            return;
        if (!segment.gunfish.effectMap.ContainsKey(EffectType.SharkMode)) {
            segment.gunfish.Hit(new FishHitObject(segment.index, collision.contacts[0].point, -collision.contacts[0].normal, gunfish.gameObject, segment.gunfish.statusData.health, 10f));
        }
    }
}

// NOTE: consider using composition over inheritance here.
public class Counter_Effect : Effect {
    public int counter;

    public Counter_Effect(Gunfish gunfish, int counter) : base(gunfish) {
        this.counter = counter;
    }

    public override void Merge(Effect effect) {
        // eat that friggin effect
        Counter_Effect t_effect = (Counter_Effect)effect;
        counter += t_effect.counter;
    }

    public override void Update() {
        base.Update();
        if (counter <= 0)
            gunfish.RemoveEffect(effectType);
    }
}

[Serializable]
public class NoMove_Effect : Counter_Effect {

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