using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GunfishGamePlayerReference : PlayerReference {

    public int gunfishIndex;
    public float lastHitTimestamp = -1;
    public Player lastHitter;
    public int totalFish;

    public GunfishGamePlayerReference(Player player, TeamReference team) : base(player, team) { }

    public override string GetStatsText() {
        return $"{gunfishIndex + 1}/{totalFish}";
    }
}

public class GunfishGameMatchManager : MatchManager<GunfishGamePlayerReference, TeamReference> {
    public GunfishDataList fishProgression;

    [HideInInspector]
    public PelicanSpawner pelicanSpawner;

    static float lastHitThreshold = 4f;

    Player matchWinner;
    bool suddenDeathActive;
    HashSet<Player> suddenDeathContenders;

    public override void Initialize(GameParameters parameters) {
        pelicanSpawner = GetComponentInChildren<PelicanSpawner>();
        skipLastStats = true;
        base.Initialize(parameters);
    }

    protected override void AddPlayerReference(Player player, TeamReference teamRef) {
        playerReferences[player] = new GunfishGamePlayerReference(player, teamRef) {
            totalFish = fishProgression.gunfishes.Count
        };
    }

    public override void StartLevel() {
        base.StartLevel();
        pelicanSpawner.FetchSpawnZones();
        pelicanSpawner.active = false;
        suddenDeathActive = false;
        suddenDeathContenders = null;
        matchWinner = null;
    }

    public override void SetUpPlayer(Player player) {
        base.SetUpPlayer(player);
        playerReferences[player].gunfishIndex = 0;
        playerReferences[player].lastHitTimestamp = -1;
        playerReferences[player].lastHitter = null;
    }

    protected override IEnumerator CoSpawnPlayer(Player player) {
        yield return new WaitForSeconds(spawnDelay);
        Transform currentSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        float maxDistance = float.MinValue;
        float distance;
        foreach (var spawnPoint in spawnPoints) {
            distance = float.MaxValue;
            bool skip = true;
            foreach (var activePlayer in parameters.activePlayers) {
                if (activePlayer.Gunfish.RootSegment == null) {
                    continue;
                } else {
                    skip = false;
                }
                var playerDist = activePlayer.Gunfish.GetPosition();
                if (playerDist.HasValue)
                    distance = Mathf.Min(distance, Vector2.Distance(spawnPoint.position, playerDist.Value));
            }
            if (skip == false && distance > maxDistance) {
                maxDistance = distance;
                currentSpawnPoint = spawnPoint;
            }
        }
        player.gunfishData = fishProgression.gunfishes[playerReferences[player].gunfishIndex];
        player.SpawnGunfish(currentSpawnPoint.position);
        UpdatePlayerProgressUI(player);
        yield return base.CoSpawnPlayer(player);
    }

    public override void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead) {
        base.HandleFishDamage(fishHit, gunfish, alreadyDead);
        if (alreadyDead || endingLevel) {
            return;
        }

        GunfishGamePlayerReference victimRef = playerReferences[gunfish.player];

        Gunfish sourceGunfish = fishHit.source.GetComponent<Gunfish>();
        sourceGunfish = sourceGunfish ?? fishHit.source.GetComponent<Gun>()?.gunfish;
        if (sourceGunfish == gunfish) {
            sourceGunfish = null;
        }
        if (sourceGunfish != null) {
            victimRef.lastHitTimestamp = Time.time;
            victimRef.lastHitter = sourceGunfish.player;
        }

        if (gunfish.statusData.health > 0) {
            return;
        }

        if ((Time.time - victimRef.lastHitTimestamp) <= lastHitThreshold && victimRef.lastHitter != null) {
            sourceGunfish = victimRef.lastHitter.Gunfish;
        }

        MarqueeManager.Instance.PlayRandomQuip(QuipType.PlayerDeath);

        if (sourceGunfish == null) {
            // suicide / hazard death: no tier advance for anyone
            return;
        }

        GunfishGamePlayerReference killerRef = playerReferences[sourceGunfish.player];
        if (killerRef.gunfishIndex >= fishProgression.gunfishes.Count - 1) {
            // killer was already on the final fish (Anglerfish) -- this kill wins the match
            DeclareWinner(sourceGunfish.player);
            EndLevel();
            return;
        }

        killerRef.gunfishIndex++;
        GunfishData nextFish = fishProgression.gunfishes[killerRef.gunfishIndex];
        sourceGunfish.player.gunfishData = nextFish;
        sourceGunfish.SwapFish(nextFish);
        // SwapFish() rebuilds the fish body via Gunfish.Spawn() internally, which does not reapply the
        // per-player outline color -- only Player.SpawnGunfish() does that. Reapply it here to match.
        var color = PlayerManager.Instance.playerColors[sourceGunfish.player.PlayerNumber];
        sourceGunfish.gunfishRenderer.LineRenderer.material.SetColor("_OutlineColor", color);
        UpdatePlayerProgressUI(sourceGunfish.player);
    }

    public override void OnPlayerDeath(Player player) {
        base.OnPlayerDeath(player);
        if (endingLevel) {
            return;
        }

        if (suddenDeathActive && suddenDeathContenders.Contains(player)) {
            HandleSuddenDeathElimination(player);
            return;
        }

        // Progress is monotonic -- dying (by any cause) never demotes a tier, just respawns at the current one.
        playerReferences[player].lastHitTimestamp = -1;
        playerReferences[player].lastHitter = null;
        SpawnPlayer(player);
    }

    private void HandleSuddenDeathElimination(Player player) {
        suddenDeathContenders.Remove(player);
        if (suddenDeathContenders.Count == 1) {
            DeclareWinner(suddenDeathContenders.First());
            EndLevel();
        } else if (suddenDeathContenders.Count == 0) {
            // Simultaneous elimination: fall back to whichever tied leader landed the more recent hit before dying.
            GunfishGamePlayerReference fallback = playerReferences.Values.OrderByDescending(p => p.lastHitTimestamp).First();
            DeclareWinner(fallback.player);
            EndLevel();
        }
        // else: 2+ contenders remain, sudden death continues; the eliminated contender doesn't respawn.
    }

    private void DeclareWinner(Player winner) {
        matchWinner = winner;
    }

    protected override void EndLevel() {
        pelicanSpawner.active = false;
        base.EndLevel();
    }

    public override void OnTimerFinish() {
        base.OnTimerFinish();
        if (endingLevel) {
            return;
        }

        int maxTier = playerReferences.Values.Max(p => p.gunfishIndex);
        List<GunfishGamePlayerReference> leaders = playerReferences.Values.Where(p => p.gunfishIndex == maxTier).ToList();

        if (leaders.Count == 1) {
            DeclareWinner(leaders[0].player);
            EndLevel();
            return;
        }

        suddenDeathActive = true;
        suddenDeathContenders = leaders.Select(l => l.player).ToHashSet();
        MarqueeManager.Instance.PlayRandomQuip(QuipType.Pelicans);
        pelicanSpawner.active = true;
    }

    public override void ShowEndGameStats() {
        base.ShowEndGameStats();

        string winnerText = "No one wins?";
        TeamReference winningTeam = null;
        if (matchWinner != null) {
            winningTeam = playerReferences[matchWinner].team;
            winnerText = $"{winningTeam.GetTitle()} wins!";
            MarqueeManager.Instance.PlayPlayerWinQuip(matchWinner);
        }

        List<GunfishGamePlayerReference> players = playerReferences.Values.OrderByDescending(x => x.gunfishIndex).ToList();
        ui.HideWidgets();
        statsUI.ShowStats(winnerText, players, winningTeam, new Dictionary<GunfishGamePlayerReference, string>(), "", final: true, showTeam: false);
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }

    private void UpdatePlayerProgressUI(Player player) {
        GunfishGamePlayerReference playerRef = playerReferences[player];
        GunfishData currentFish = fishProgression.gunfishes[playerRef.gunfishIndex];
        ui.OnFishProgressChange(player, currentFish, playerRef.gunfishIndex, fishProgression.gunfishes.Count);
    }

    public override int GetPlayerScore(Player player) {
        if (player == null || !playerReferences.ContainsKey(player)) {
            return -1;
        }
        return playerReferences[player].gunfishIndex;
    }

    public override void SetPlayerRating(Player player, int rating) {
        if (player == null || !playerReferences.ContainsKey(player)) {
            return;
        }
        playerReferences[player].rating = rating;
    }

    public override int GetPlayerRating(Player player) {
        if (player == null || !playerReferences.ContainsKey(player)) {
            return 0;
        }
        return playerReferences[player].rating;
    }
}
