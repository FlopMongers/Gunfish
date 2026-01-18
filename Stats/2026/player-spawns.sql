SELECT
    mr.MatchResultId,
    mr.GameMode,
    mr.StartTime AS MatchStartTime,
    mr.EndTime AS MatchEndTime,
    mr.LevelCount,
    mr.PlayerCount,
    lvr.LevelResultId,
    lvr.LevelName,
    lvr.StartTime AS LevelStartTime,
    lvr.EndTime AS LevelEndTime,
    ps.SpawnId,
    ps.PlayerId,
    ps.SpawnTime,
    ps.X,
    ps.Y
FROM
    LevelResult lvr
LEFT JOIN
    PlayerSpawn ps
    ON ps.SpawnTime BETWEEN lvr.StartTime AND lvr.EndTime
LEFT JOIN
    MatchResult mr
    ON mr.MatchResultId = lvr.MatchResultId
