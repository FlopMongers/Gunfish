
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;

public delegate void FXEvent(FX_Object obj);

public class FX_Object : MonoBehaviour {
    public FXEvent Destroy_Event;

    //public GameEvent OnDead;
    public float pitch_range = 0.2f, amp_range = 0.02f;
    public float vol = -1f;
    public bool live_forever;
    public float lifetime = 0, MaxLifetime;

    bool destroyed;

    protected bool init;
    public bool parent = true;

    public FXType fx_type = FXType.Default, track_fx_type = FXType.Default;

    public AudioMixerGroup mixerGroup;
    // Start is called before the first frame update
    protected void Awake() {

        LookAtConstraint lac = GetComponent<LookAtConstraint>();
        if (lac) {
            ConstraintSource src = new ConstraintSource();
            src.sourceTransform = UnityEngine.GameObject.FindGameObjectWithTag("Player").transform;
            src.weight = 1;
            lac.AddSource(src);
            lac.constraintActive = true;
        }

        float max_audio_len = 0, max_part_len = 0;
        foreach (AudioSource aud in GetComponentsInChildren<AudioSource>()) {
            if (!aud.clip)
                continue;
            if (vol != -1)
                aud.volume = vol;
            aud.outputAudioMixerGroup = mixerGroup;
            aud.pitch += Random.Range(-pitch_range, pitch_range);
            aud.volume += Random.Range(-amp_range, 0);
            if (mixerGroup)
                aud.outputAudioMixerGroup = mixerGroup;
            if (aud.clip.length > max_audio_len)
                max_audio_len = aud.clip.length;
        }
        foreach (ParticleSystem part in GetComponentsInChildren<ParticleSystem>()) {
            if (part.main.duration > max_part_len)
                max_part_len = part.main.duration;
        }
        if (live_forever) { lifetime = float.MaxValue; }
        else {
            MaxLifetime = Mathf.Max(max_audio_len, max_part_len);
            lifetime = MaxLifetime;
            //Destroy(gameObject, lifetime);
        }
    }

    public virtual void Play() {
        lifetime = MaxLifetime;
        foreach (AudioSource aud in GetComponentsInChildren<AudioSource>()) {
            if (!aud.clip)
                continue;
            if (vol != -1)
                aud.volume = vol;
            aud.Play();
        }
        foreach (ParticleSystem part in GetComponentsInChildren<ParticleSystem>()) {
            part.Play();
        }
    }

    public virtual void Replay() {
        Play();
    }

    private void Update() {
        if (!parent)
            return;
        if (!live_forever)
            lifetime -= Time.deltaTime;
        if (lifetime <= 0 && destroyed == false) {
            destroyed = true;
            Destroy_Event?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void Kill() {
        this.lifetime = -1;
    }
}