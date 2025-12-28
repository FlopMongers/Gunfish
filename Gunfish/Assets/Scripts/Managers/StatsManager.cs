using System;
using SQLite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchResult
{
    [PrimaryKey, AutoIncrement]
    public int MatchResultId { get; set; }
    public string GameMode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int LevelCount { get; set; }
    public int PlayerCount { get; set; }

    [Ignore]
    public Dictionary<Player, PlayerMatchResult> PlayerMatchResults { get; set; }

    [Ignore]
    public List<LevelResult> LevelResults { get; set; }
}

public class LevelResult
{
    [PrimaryKey, AutoIncrement]
    public int LevelResultId { get; set; }
    public int MatchResultId { get; set; }
    public string LevelName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class PlayerMatchResult
{
    [PrimaryKey, AutoIncrement]
    public int PlayerMatchResultId { get; set; }
    public int MatchResultId { get; set; }
    public int PlayerId { get; set; }
    public int PlayerTeam { get; set; }
    public string PlayerFish { get; set; }
    public int Score { get; set; }
    public int Rating { get; set; }
}

// These will be associated with matches and levels
// via a data prep step before analysis
public class PlayerDamage
{
    [PrimaryKey, AutoIncrement]
    public int DamageId { get; set; }
    public int PlayerId { get; set; }
    public DateTime DamageTime { get; set; }
    public int SourceId { get; set; }
    public string SourceType { get; set; }
    public float Amount { get; set; }
    public bool IsFatal { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
}

public class PlayerSpawn
{
    [PrimaryKey, AutoIncrement]
    public int SpawnId { get; set; }
    public int PlayerId { get; set; }
    public DateTime SpawnTime { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
}

public class StatsManager : MonoBehaviour
{
    static StatsManager instance;
    public static StatsManager Instance
    {
        get
        {
            return instance;
        }
    }

    SQLiteConnection dbConnection;

    public static void LogMatchResults(MatchResult matchResult)
    {
        if (instance == null)
        {
            Debug.LogError("StatsManager instance is null. Cannot save match results.");
            return;
        }
        Instance.WriteMatchResults(matchResult);
    }

    private void WriteMatchResults(MatchResult matchResult)
    {
        dbConnection.Insert(matchResult);
        foreach (var pmr in matchResult.PlayerMatchResults.Values)
        {
            pmr.MatchResultId = matchResult.MatchResultId;
            dbConnection.Insert(pmr);
        }

        foreach (var levelResult in matchResult.LevelResults)
        {
            levelResult.MatchResultId = matchResult.MatchResultId;
            dbConnection.Insert(levelResult);

        }
    }

    // Generic Log method for basic stat types
    public static void LogStat<T>(T stat)
    {
        if (instance == null)
        {
            Debug.LogError("StatsManager instance is null. Cannot log stat.");
            return;
        }
        Instance.dbConnection.Insert(stat);
    }

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        dbConnection = new SQLiteConnection($"{Application.persistentDataPath}/stats.db");
        Debug.Log($"Stats DB Path: {Application.persistentDataPath}/stats.db");
        dbConnection.CreateTable<MatchResult>();
        dbConnection.CreateTable<PlayerMatchResult>();
        dbConnection.CreateTable<LevelResult>();
        dbConnection.CreateTable<PlayerSpawn>();
        dbConnection.CreateTable<PlayerDamage>();
    }

}
