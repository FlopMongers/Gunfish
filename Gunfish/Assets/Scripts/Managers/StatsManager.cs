using SQLite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string GameMode { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public int LevelCount { get; set; }
    public int PlayerCount { get; set; }
}

public class LevelResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MatchResultId { get; set; }
    public string LevelName { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
}

public class PlayerMatchResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MatchResultId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerFish { get; set; }
    public int TotalScore { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int Rating { get; set; }
}

public class PlayerLevelResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MatchResultId { get; set; }
    public int LevelResultId { get; set; }
    public int PlayerId { get; set; }
    public int Score { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
}


public struct LevelResultsData
{
    public LevelResult levelResult;
    public List<PlayerLevelResult> playerLevelResults;
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

    public static void SaveMatchResults(MatchResult matchResult, List<PlayerMatchResult> playerMatchResults, List<LevelResultsData> levelResultsData)
    {
        if (instance == null)
        {
            Debug.LogError("StatsManager instance is null. Cannot save match results.");
            return;
        }
        Instance.WriteMatchResults(matchResult, playerMatchResults, levelResultsData);
    }

    private void WriteMatchResults(MatchResult matchResult, List<PlayerMatchResult> playerMatchResults, List<LevelResultsData> levelResultsData)
    {
        dbConnection.Insert(matchResult);
        foreach (var pmr in playerMatchResults)
        {
            pmr.MatchResultId = matchResult.Id;
            dbConnection.Insert(pmr);
        }

        foreach (var levelData in levelResultsData)
        {
            var levelResult = levelData.levelResult;
            levelResult.MatchResultId = matchResult.Id;
            dbConnection.Insert(levelResult);

            foreach (var plr in levelData.playerLevelResults)
            {
                plr.LevelResultId = levelResult.Id;
                dbConnection.Insert(plr);
            }
        }
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
        dbConnection.CreateTable<PlayerLevelResult>();
    }

}
