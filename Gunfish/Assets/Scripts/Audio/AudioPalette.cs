using UnityEngine;

[CreateAssetMenu(fileName = "AudioPalette", menuName = "Scriptable Objects/Audio Palette")]
public class AudioPalette : ScriptableObject {
    public AudioClip[] clips;
    public float defaultVol = 1f;
}