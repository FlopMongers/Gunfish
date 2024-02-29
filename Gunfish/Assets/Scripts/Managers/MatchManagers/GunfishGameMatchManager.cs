using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishGamePlayerReference : PlayerReference {

    public GunfishData startingFish;
    public int gunfishIndex;
    public float lastKillTimestamp;

    public GunfishGamePlayerReference(Player player, TeamReference team) : base(player, team) { }
}

public class GunfishGameMatchManager : MatchManager<GunfishGamePlayerReference, TeamReference> 
{

}
