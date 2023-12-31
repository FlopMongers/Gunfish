using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TrackSetLabel {
    Menu,
    Gameplay,
}

[System.Serializable]
public struct TrackEnumToObj {
    public TrackSetLabel label;
    public TrackSet set;
}

public class MusicManager : PersistentSingleton<MusicManager> {

    [SerializeField]
    private List<TrackEnumToObj> trackSetMap;
    [SerializeField]
    private Dictionary<TrackSetLabel, TrackSet> musicTrackDictionary;
    [SerializeField]
    private TrackSetLabel defaultTrackSet;
    [SerializeField]
    private bool playOnStart;
    [SerializeField]
    private AnimationCurve fadeInCurve;

    [SerializeField]
    [Range(0f, 5f)]
    public float fadeTime;

    private Queue<AudioClip> musicQueue = new();
    private AudioSource[] audioSources;
    private bool doFade = true;
    private int activeSourceIndex = 0;
    private int targetSourceIndex = 0;
    bool transitioning = false;

    private float clipTimer = 0f;

    protected void Update() {

        if (GameManager.debug && Input.GetKeyDown(KeyCode.M)) {
            clipTimer = 0f;
        }

        if (clipTimer >= 0f) {
            clipTimer -= Time.deltaTime;
        } else if (!transitioning) {
            StartNextTrack();
        }
    }

    public override void Initialize() {
        base.Initialize();
        InitializeAudioSources();
        InitializeMusicTrackDictionary();

        if (playOnStart) {
            PlayTrackSet(defaultTrackSet);
        }
    }

    private void InitializeAudioSources() {
        audioSources = new AudioSource[] {
            gameObject.AddComponent<AudioSource>(),
            gameObject.AddComponent<AudioSource>()
        };

        foreach (var audioSource in audioSources) {
            audioSource.loop = false;
        }
    }

    private void InitializeMusicTrackDictionary() {
        musicTrackDictionary = new();
        foreach (var trackAndClip in trackSetMap) {
            musicTrackDictionary.Add(trackAndClip.label, trackAndClip.set);
        }
    }

    public void PlayTrackSet(TrackSetLabel setLabel) {
        musicQueue.Clear();

        TrackSet set;
        if (!musicTrackDictionary.TryGetValue(setLabel, out set)) {
            string message = $"{setLabel} could not be found in music map. Please check to see if it's added in the MusicManager component";
            throw new KeyNotFoundException(message);
        }

        foreach (AudioClip clip in set.tracks.OrderBy(x => Random.value)) {
            musicQueue.Enqueue(clip);
        }
        doFade = set.doFade;

        StartNextTrack();
    }

    private void StartNextTrack() {
        AudioClip audioClip;
        if (!musicQueue.TryDequeue(out audioClip)) {
            return;
        }
        musicQueue.Enqueue(audioClip);

        targetSourceIndex = (activeSourceIndex + 1) % audioSources.Length;
        audioSources[targetSourceIndex].clip = audioClip;
        audioSources[targetSourceIndex].Play();

        clipTimer = audioClip.length;

        if (doFade) {
            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade() {
        transitioning = true;
        float t = 0f;
        while (t < fadeTime) {
            var activeVolume = fadeInCurve.Evaluate(fadeTime - t);
            var targetVolume = fadeInCurve.Evaluate(t);

            audioSources[activeSourceIndex].volume = activeVolume;
            audioSources[targetSourceIndex].volume = targetVolume;

            t += Time.deltaTime / fadeTime;
            yield return new WaitForEndOfFrame();
        }
        audioSources[activeSourceIndex].volume = 0f;
        audioSources[targetSourceIndex].volume = 1f;
        activeSourceIndex = targetSourceIndex;
        transitioning = false;
    }

    private void OnGUI() {
        if (!GameManager.debug)
            return;

        GUI.TextField(new Rect(0f, 0f, 200f, 20f), "Press M to change music track.");
    }
}