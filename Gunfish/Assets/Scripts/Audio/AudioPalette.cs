using UnityEngine;

[CreateAssetMenu(fileName = "AudioPalette", menuName = "ScriptableObjects/AudioPalette", order = 1)]
public class AudioPalette : ScriptableObject
{
    public AudioClip[] clips;
    public float defaultVol = 1f;
}