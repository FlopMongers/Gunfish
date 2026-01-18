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
    pd.DamageId,
    pd.PlayerId,
    pd.DamageTime,
    pd.SourceId,
    pd.SourceType,
    pd.Amount,
    pd.IsFatal,
    pd.IsSelfInflicted,
    pd.X,
    pd.Y
FROM
    LevelResult lvr
LEFT JOIN
    PlayerDamage pd
    ON pd.DamageTime BETWEEN lvr.StartTime AND lvr.EndTime
LEFT JOIN
    MatchResult mr
    ON mr.MatchResultId = lvr.MatchResultId
