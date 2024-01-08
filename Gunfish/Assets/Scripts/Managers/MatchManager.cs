using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour {
    protected GameParameters parameters;
    protected int currentLevel;

    protected List<Transform> spawnPoints;

    private int nextLevelIndex;
    private bool done;

    public LevelTimer timer;
    static float levelDuration = 120;

    public virtual void Initialize(GameParameters parameters) {
        this.parameters = parameters;
        spawnPoints = new List<Transform>();
        LevelManager.Instance.OnFinishLoadLevel += StartLevel;
        LevelManager.Instance.OnStartPlay += StartPlay;
        timer = timer ?? GetComponentInChildren<LevelTimer>();
        if (timer != null) {
            timer.levelDuration = levelDuration;
            timer.OnTimerFinish += OnTimerFinish;
        }
        NextLevel();
    }

    public void TearDown() {
        LevelManager.Instance.OnFinishLoadLevel -= StartLevel;
        LevelManager.Instance.OnStartPlay -= StartPlay;
    }

    public virtual void SpawnPlayer(Player player) {
        // add player's fish to camera group
        GameCamera.Instance?.targetGroup.AddMember(player.Gunfish.MiddleSegment.transform, 1, 1);
    }

    public virtual void StartLevel() {
        InitializeSpawnPoints();
        FreezeFish(true);
    }


    public virtual void StartPlay() {
        timer?.StartTime();
        FreezeFish(false);
    }

    private void InitializeSpawnPoints() {
        spawnPoints = new List<Transform>();
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("Spawn")) {
            spawnPoints.Add(spawnPoint.transform);
        }
    }

    public void FreezeFish(bool freeze) {
        foreach (var player in parameters.activePlayers) {
            player.FreezeControls = freeze;
        }
    }

    public virtual void OnPlayerDeath(Player player) {
        // remove the fishy from the camera group
        // will this work? I don't know...
        GameCamera.Instance?.targetGroup.RemoveMember(null);
    }

    public virtual void ShowEndGameStats() {

    }

    public virtual void NextLevel() {
        if (nextLevelIndex < parameters.scenes.Count) {
            LevelManager.Instance.LoadLevel(parameters.scenes[nextLevelIndex], parameters.skyboxScene);
            nextLevelIndex++;
        }
        else if (done == true) {
            // NOTE destroy all players
            LevelManager.Instance.LoadMainMenu(() => { 
                GameManager.Instance.ResetGame();
                MusicManager.Instance.PlayTrackSet(TrackSetLabel.Menu);
                MainMenu.Instance.Initialize();
            });
        }
        else {
            done = true;
            LevelManager.Instance.LoadStats(() => {
                ShowEndGameStats();
            });
        }
    }

    public virtual bool ResolveHit(Gun gun, GunfishSegment segment) {
        return gun.gunfish != segment.gunfish;
    }

    public virtual void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead) {
    }

    public virtual void OnTimerFinish() { }
}