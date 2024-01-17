using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkmodeManager : Singleton<SharkmodeManager>
{
    public HashSet<Gunfish> gunfishes = new HashSet<Gunfish>();
    TrackSetLabel trackSetLabel;

    public void UpdateCounter(Gunfish gunfish, bool add) {
        if (add) {
            gunfishes.Add(gunfish);
        }
        else {
            gunfishes.Remove(gunfish);
        }
        if (add == true && gunfishes.Count == 1) {
            // start playing sharkmode
            print("oh yeah sharkmode time!");
            trackSetLabel = MusicManager.Instance.currentTrackSetLabel;
            MusicManager.Instance.PlayTrackSet(TrackSetLabel.Sharkmode);
        }
        else if (gunfishes.Count == 0) {
            // stop playing sharkmode
            print("yep, no more sharkmode");
            MusicManager.Instance.PlayTrackSet(trackSetLabel);
        }
    }
}
