/*
Re-create player results per level based on death and spawn data.
Rely heavily on the fact that levels will never be concurrent.
*/

WITH PlayerDeathParsed AS (
    SELECT
        Id,
        PlayerId,
        TimeOfDeath AS DeathTime,
        CauseOfDeath,
        -- Parse CauseOfDeath string like 'Player 1' to get PlayerId of killer
        CASE
            WHEN CauseOfDeath LIKE 'Player %' THEN CAST(SUBSTR(CauseOfDeath, 8) AS INTEGER)-1
            ELSE NULL
        END AS KillerPlayerID
    FROM
        PlayerDeath
),
LevelSpawns AS (
    SELECT
        lvr.Id AS LevelResultId,
        ps.PlayerId,
        COUNT(ps.Id) AS Spawns
    FROM
        LevelResult lvr
    LEFT JOIN
        PlayerSpawn ps
        ON ps.TimeOfSpawn BETWEEN lvr.StartTime AND lvr.EndTime
    GROUP BY
        lvr.Id, ps.PlayerId
),
LevelDeaths AS (
    SELECT
        lvr.Id AS LevelResultId,
        pd.PlayerId,
        COUNT(pd.Id) AS Deaths
    FROM
        LevelResult lvr
    LEFT JOIN
        PlayerDeathParsed pd
        ON pd.DeathTime BETWEEN lvr.StartTime AND lvr.EndTime
    GROUP BY
        lvr.Id, pd.PlayerId
),
LevelKills AS (
    SELECT
        lvr.Id AS LevelResultId,
        pd.KillerPlayerId AS PlayerId,
        COUNT(pd.Id) AS Kills
    FROM
        LevelResult lvr
    LEFT JOIN
        PlayerDeathParsed pd
        ON pd.DeathTime BETWEEN lvr.StartTime AND lvr.EndTime
    GROUP BY
        lvr.Id, pd.KillerPlayerId
)

SELECT
    pmr.MatchResultId,
    lvr.Id AS LevelResultId,
    pmr.PlayerId,
    pmr.TotalScore AS Score,
    ls.Spawns,
    lk.Kills,
    ld.Deaths,
    pmr.Rating,
    pmr.PlayerFish,
    pmr.PlayerTeam,
    mr.GameMode,
    mr.StartTime AS MatchStartTime,
    mr.EndTime AS MatchEndTime,
    lvr.LevelName,
    lvr.StartTime AS LevelStartTime,
    lvr.EndTime AS LevelEndTime
FROM
    PlayerMatchResult pmr
LEFT JOIN
    LevelResult lvr
    ON lvr.MatchResultId = pmr.MatchResultId
LEFT JOIN
    MatchResult mr
    ON mr.Id = pmr.MatchResultId
LEFT JOIN
    LevelSpawns ls
    ON ls.LevelResultId = lvr.Id AND ls.PlayerID = pmr.PlayerID
LEFT JOIN
    LevelDeaths ld
    ON ld.LevelResultId = lvr.Id AND ld.PlayerID = pmr.PlayerID
LEFT JOIN
    LevelKills lk
    ON lk.LevelResultId = lvr.Id AND lk.PlayerID = pmr.PlayerID
