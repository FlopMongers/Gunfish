using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMatchManager : MatchManager
{
    Dictionary<Player, int> playerScores = new Dictionary<Player, int>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(GameParameters parameters)
    {
        foreach (var player in parameters.activePlayers)
        {
            playerScores[player] = 0;
        }
        base.Initialize(parameters);
    }
}
