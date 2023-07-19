using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MusicTrack {
    AllStar,
    CoryInTheHouse,
}

[System.Serializable]
public struct MusicTrackToClip {
    public MusicTrack track;
    public AudioClip clip;
}

public class MusicManager : PersistentSingleton<MusicManager> {
    
    [SerializeField]
    private List<MusicTrackToClip> musicTrackMap;
    [SerializeField]
    private Dictionary<MusicTrack, AudioClip> musicTrackDictionary;
    [SerializeField]
    private MusicTrack defaultTrack;
    [SerializeField]
    private bool playOnStart;
    [SerializeField]
    private AnimationCurve fadeInCurve;
    
    [SerializeField]
    [Range(0f, 5f)]
    public float fadeTime;

    private Queue<AudioClip> musicQueue = new();
    private AudioSource[] audioSources;
    private bool transitioning = false;
    private int activeSourceIndex = 0;

    private void Start() {
        InitializeAudioSources();
        InitializeMusicTrackDictionary();

        if (playOnStart) {
            StartTrack(MusicTrack.AllStar);
        }
    }

    private void Update() {
        if (GameManager.debug) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                var index = activeSourceIndex + (transitioning ? 1 : 0) + musicQueue.Count;
                if (index % 2 == 0) {
                    StartTrack(MusicTrack.AllStar);
                } else {
                    StartTrack(MusicTrack.CoryInTheHouse);
                }
            }
        }
    }

    private void InitializeAudioSources() {
        audioSources = new AudioSource[] {
            gameObject.AddComponent<AudioSource>(),
            gameObject.AddComponent<AudioSource>()
        };
        
        foreach (var audioSource in audioSources) {
            audioSource.loop = true;
        }
    }

    private void InitializeMusicTrackDictionary() {
        musicTrackDictionary = new();
        foreach (var trackAndClip in musicTrackMap) {
            musicTrackDictionary.Add(trackAndClip.track, trackAndClip.clip);
        }
    }

    public void StartTrack(MusicTrack track) {
        if (!musicTrackDictionary.ContainsKey(track)) {
            string message = $"{track.ToString()} could not be found in music map. Please check to see if it's added in the MusicManager component";
            throw new KeyNotFoundException(message);
        }

        AudioClip clip;
        if (musicTrackDictionary.TryGetValue(track, out clip)) {
            musicQueue.Enqueue(clip);
            if (!transitioning) {
                transitioning = true;
                StartCoroutine(Fade());
            }
        } else {
            string message = $"{track.ToString()} did not contain a value. Please set one in the MusicManager component.";
            throw new UnassignedReferenceException(message);
        }
    }

    private IEnumerator Fade() {
        while (musicQueue.Count > 0) {
            AudioClip audioClip;
            if (!musicQueue.TryDequeue(out audioClip)) {
                continue;
            }

            var targetSourceIndex = (activeSourceIndex + 1) % audioSources.Length;
            audioSources[targetSourceIndex].clip = audioClip;
            audioSources[targetSourceIndex].Play();

            float t = 0f;
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
        }
        transitioning = false;
    }

    private void OnGUI() {
        if (!GameManager.debug) return;

        GUILayout.TextField("Press Space to change music track.");
    }
}