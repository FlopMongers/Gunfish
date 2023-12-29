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
    private bool do_fade = true;
    private int activeSourceIndex = 0;

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

        do_fade = set.do_fade;
        StartCoroutine(PlayTracks());
    }

    private IEnumerator PlayTracks() {
        while (musicQueue.Count > 0) {
            AudioClip audioClip;
            if (!musicQueue.TryDequeue(out audioClip)) {
                continue;
            }
            musicQueue.Enqueue(audioClip);

            var targetSourceIndex = (activeSourceIndex + 1) % audioSources.Length;
            audioSources[targetSourceIndex].clip = audioClip;
            audioSources[targetSourceIndex].Play();

            float t = do_fade ? 0f : 1f;
            while (t < 1f) {
                var activeVolume = fadeInCurve.Evaluate(1 - t);
                var targetVolume = fadeInCurve.Evaluate(t);

                audioSources[activeSourceIndex].volume = activeVolume;
                audioSources[targetSourceIndex].volume = targetVolume;

                t += Time.deltaTime / fadeTime;
                yield return new WaitForEndOfFrame();
            }
            audioSources[activeSourceIndex].volume = 0f;
            audioSources[targetSourceIndex].volume = 1f;
            activeSourceIndex = targetSourceIndex;

            float clip_len = audioClip.length;
            while (clip_len > 0f) {
                clip_len -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void OnGUI() {
        if (!GameManager.debug)
            return;

        GUI.TextField(new Rect(0f, 0f, 200f, 20f), "Press M to change music track.");
    }
}