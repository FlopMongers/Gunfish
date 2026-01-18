/*
Re-create player results per level based on death and spawn data.
Rely heavily on the fact that levels will never be concurrent.
*/

WITH LevelSpawns AS (
    SELECT
        lvr.LevelResultId,
        ps.PlayerId,
        COUNT(ps.SpawnId) AS Spawns
    FROM
        LevelResult lvr
    LEFT JOIN
        PlayerSpawn ps
        ON ps.SpawnTime BETWEEN lvr.StartTime AND lvr.EndTime
    GROUP BY
        lvr.LevelResultId, ps.PlayerId
),
LevelDeaths AS (
    SELECT
        lvr.LevelResultId,
        pd.PlayerId,
        COUNT(pd.DamageId) AS Deaths
    FROM
        LevelResult lvr
    LEFT JOIN
        PlayerDamage pd
        ON pd.DamageTime BETWEEN lvr.StartTime AND lvr.EndTime
    WHERE
        pd.IsFatal = 1
    GROUP BY
        lvr.LevelResultId, pd.PlayerId
),
LevelKills AS (
    SELECT
        lvr.LevelResultId,
        pd.SourceId AS PlayerId,
        COUNT(pd.DamageId) AS Kills
    FROM
        LevelResult lvr
    LEFT JOIN
        PlayerDamage pd
        ON pd.DamageTime BETWEEN lvr.StartTime AND lvr.EndTime
    WHERE
        pd.SourceType = 'Player'
        AND pd.IsFatal = 1
    GROUP BY
        lvr.LevelResultId, pd.SourceId
)

SELECT
    pmr.MatchResultId,
    lvr.LevelResultId,
    pmr.PlayerId,
    pmr.Score,
    COALESCE(ls.Spawns, 0) AS Spawns,
    COALESCE(lk.Kills, 0) AS Kills,
    COALESCE(ld.Deaths, 0) AS Deaths,
    COALESCE(pmr.Rating, 0) AS Rating,
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
    ON mr.MatchResultId = pmr.MatchResultId
LEFT JOIN
    LevelSpawns ls
    ON ls.LevelResultId = lvr.LevelResultId AND ls.PlayerID = pmr.PlayerID
LEFT JOIN
    LevelDeaths ld
    ON ld.LevelResultId = lvr.LevelResultId AND ld.PlayerID = pmr.PlayerID
LEFT JOIN
    LevelKills lk
    ON lk.LevelResultId = lvr.LevelResultId AND lk.PlayerID = pmr.PlayerID
