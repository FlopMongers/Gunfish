WITH LifeStarts AS (
    SELECT
        ROW_NUMBER() OVER (PARTITION BY pmr.MatchResultId, lvr.LevelResultId, pmr.PlayerId ORDER BY ps.SpawnTime) AS LevelLifeNumber,
        pmr.MatchResultId,
        lvr.LevelResultId,
        pmr.PlayerId,
        ps.SpawnId,
        ps.SpawnTime AS LifeStartTime,
        ps.X AS SpawnX,
        ps.Y AS SpawnY
    FROM
        PlayerMatchResult pmr
    LEFT JOIN
        LevelResult lvr
        ON lvr.MatchResultId = pmr.MatchResultId
    LEFT JOIN
        MatchResult mr
        ON mr.MatchResultId = pmr.MatchResultId
    LEFT JOIN
        PlayerSpawn ps
        ON ps.SpawnTime BETWEEN lvr.StartTime AND lvr.EndTime
        AND ps.PlayerId = pmr.PlayerId
), LifeEnds AS (
    SELECT
        ROW_NUMBER() OVER (PARTITION BY pmr.MatchResultId, lvr.LevelResultId, pmr.PlayerId ORDER BY pd.DamageTime) AS LevelDeathNumber,
        pmr.MatchResultId,
        lvr.LevelResultId,
        pmr.PlayerId,
        pd.DamageId AS DeathId,
        pd.DamageTime AS LifeEndTime,
        pd.SourceId AS KillerId,
        pd.SourceType AS KillerType,
        pd.X AS DeathX,
        pd.Y AS DeathY
    FROM
        PlayerMatchResult pmr
    LEFT JOIN
        LevelResult lvr
        ON lvr.MatchResultId = pmr.MatchResultId
    LEFT JOIN
        PlayerDamage pd
        ON pd.DamageTime BETWEEN lvr.StartTime AND lvr.EndTime
        AND pd.PlayerId = pmr.PlayerId AND pd.IsFatal = 1
), PlayerLives AS (
    SELECT
        ROW_NUMBER() OVER () AS LifeId,
        ls.MatchResultId,
        ls.LevelResultId,
        ls.PlayerId,
        ls.SpawnId,
        ls.LevelLifeNumber,
        ls.LifeStartTime,
        ls.SpawnX,
        ls.SpawnY,
        le.DeathId,
        le.LevelDeathNumber,
        le.LifeEndTime,
        le.KillerId,
        le.KillerType,
        le.DeathX,
        le.DeathY
    FROM
        LifeStarts ls
    LEFT JOIN
        LifeEnds le
        ON le.MatchResultId = ls.MatchResultId
        AND le.LevelResultId = ls.LevelResultId
        AND le.PlayerId = ls.PlayerId
        AND le.LevelDeathNumber = ls.LevelLifeNumber
), LifeKills AS (
    SELECT
        pl.LifeId,
        pd.SourceId AS PlayerId,
        COUNT(pd.DamageId) AS Kills
    FROM
        PlayerLives pl
    LEFT JOIN
        PlayerDamage pd
        ON pd.DamageTime BETWEEN pl.LifeStartTime AND pl.LifeEndTime
    WHERE
        pd.SourceType = 'Player'
        AND pd.IsFatal = 1
    GROUP BY
        pl.LifeId, pd.SourceId
), LifeDamageDealt AS (
    SELECT
        pl.LifeId,
        pd.PlayerId,
        SUM(pd.Amount) AS TotalDamageDealt,
        SUM(pd.Amount) / COUNT(pd.DamageId) AS AverageDamageDealtPerHit
    FROM
        PlayerLives pl
    LEFT JOIN
        PlayerDamage pd
        ON pd.DamageTime BETWEEN pl.LifeStartTime AND pl.LifeEndTime
        AND pd.SourceId = pl.PlayerId AND pd.SourceType = 'Player'
    GROUP BY
        pl.LifeId, pd.PlayerId
), LifeDamageTaken AS (
    SELECT
        pl.LifeId,
        pd.PlayerId,
        SUM(pd.Amount) AS TotalDamageTaken,
        SUM(pd.Amount) / COUNT(pd.DamageId) AS AverageDamageTakenPerHit
    FROM
        PlayerLives pl
    LEFT JOIN
        PlayerDamage pd
        ON pd.DamageTime BETWEEN pl.LifeStartTime AND pl.LifeEndTime
        AND pd.PlayerId = pl.PlayerId
    GROUP BY
        pl.LifeId, pd.PlayerId
)

SELECT
    pl.LifeId,
    pl.MatchResultId,
    pl.LevelResultId,
    pl.PlayerId,
    pl.SpawnId,
    pl.LevelLifeNumber,
    pl.LifeStartTime,
    pl.SpawnX,
    pl.SpawnY,
    pl.DeathId,
    pl.LevelDeathNumber,
    pl.LifeEndTime,
    pl.KillerId,
    pl.KillerType,
    pl.DeathX,
    pl.DeathY,
    COALESCE(lk.Kills, 0) AS Kills,
    COALESCE(ldd.TotalDamageDealt, 0) AS TotalDamageDealt,
    COALESCE(ldd.AverageDamageDealtPerHit, 0) AS AverageDamageDealtPerHit,
    COALESCE(ldt.TotalDamageTaken, 0) AS TotalDamageTaken,
    COALESCE(ldt.AverageDamageTakenPerHit, 0) AS AverageDamageTakenPerHit
FROM
    PlayerLives pl
LEFT JOIN
    LifeKills lk
    ON lk.LifeId = pl.LifeId
LEFT JOIN
    LifeDamageDealt ldd
    ON ldd.LifeId = pl.LifeId
LEFT JOIN
    LifeDamageTaken ldt
    ON ldt.LifeId = pl.LifeId
ORDER BY
    pl.MatchResultId,
    pl.LevelResultId,
    pl.PlayerId,
    pl.LevelLifeNumber
