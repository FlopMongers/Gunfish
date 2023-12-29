using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TrackSet", menuName = "Scriptable Objects/Track Set")]
public class TrackSet : ScriptableObject {
    public List<AudioClip> tracks;
    public bool do_fade = true;
}
