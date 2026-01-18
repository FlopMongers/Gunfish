
/*
spawn_points = {
    "acid_factory": [
        (-23.63516, -8.419025),
        (5.274841, -8.399027),
        (-15.52516, -8.449028),
        (-4.605159, -8.419025),
    ],
    "barrel": [
        (-6.3, 0.9),
        (3.7, 2.2),
        (1.6, -9.1),
        (-4.78, -9.1),
    ],
    "beach": [
        (-12.24, -1.54),
        (-4.86, -1.77),
        (6.74, -4.11),
        (13.33, 1.01),
    ],
    "cargo_hold": [
        (0, 9.5),
        (7.5, 2.5),
        (-7.5, 2.5),
        (0, -5),
    ],
    "firing_range": [
        (-6.82, -4.14),
        (-7.87, -9.2),
        (9.88, -3.8),
        (21.49, -11.84),
    ],
    "valley": [
        (9.36, -14.22),
        (-1.87, -17.33),
        (-7.08, -24.84),
        (4.45, -23.13),
    ]
}
For use with a sqlite database
*/

-- CTE storing the actual spawn points for each level
WITH LEVEL_SPAWN_POINTS AS (
    SELECT 'acid_factory' AS LevelName, 0 AS PointId, -15.13516 AS SpawnX, -4.419025 AS SpawnY UNION ALL
    SELECT 'acid_factory', 1 AS PointId, 13.77484 AS SpawnX, -4.399027 AS SpawnY UNION ALL
    SELECT 'acid_factory', 2 AS PointId, -7.025159 AS SpawnX, -4.449028 AS SpawnY UNION ALL
    SELECT 'acid_factory', 3 AS PointId, 3.894841 AS SpawnX, -4.419025 AS SpawnY UNION ALL
    SELECT 'barrel', 0 AS PointId, -7.601728 AS SpawnX, -0.4296 AS SpawnY UNION ALL
    SELECT 'barrel', 1 AS PointId, 2.398272 AS SpawnX, 0.8704001 AS SpawnY UNION ALL
    SELECT 'barrel', 2 AS PointId, 0.2982715 AS SpawnX, -10.4296 AS SpawnY UNION ALL
    SELECT 'barrel', 3 AS PointId, -6.081729 AS SpawnX, -10.4296 AS SpawnY UNION ALL
    SELECT 'beach', 0 AS PointId, -12.24 AS SpawnX, -1.54 AS SpawnY UNION ALL
    SELECT 'beach', 1 AS PointId, -4.86 AS SpawnX, -1.77 AS SpawnY UNION ALL
    SELECT 'beach', 2 AS PointId, 6.74 AS SpawnX, -4.11 AS SpawnY UNION ALL
    SELECT 'beach', 3 AS PointId, 13.33 AS SpawnX, 1.01 AS SpawnY UNION ALL
    SELECT 'cargo_hold', 0 AS PointId, 0 AS SpawnX, 9.5 AS SpawnY UNION ALL
    SELECT 'cargo_hold', 1 AS PointId, 7.5 AS SpawnX, 2.5 AS SpawnY UNION ALL
    SELECT 'cargo_hold', 2 AS PointId, -7.5 AS SpawnX, 2.5 AS SpawnY UNION ALL
    SELECT 'cargo_hold', 3 AS PointId, 0 AS SpawnX, -5 AS SpawnY UNION ALL
    SELECT 'firing_range', 0 AS PointId, -6.82 AS SpawnX, -4.14 AS SpawnY UNION ALL
    SELECT 'firing_range', 1 AS PointId, -7.87 AS SpawnX, -9.2 AS SpawnY UNION ALL
    SELECT 'firing_range', 2 AS PointId, 9.88 AS SpawnX, -3.8 AS SpawnY UNION ALL
    SELECT 'firing_range', 3 AS PointId, 21.49 AS SpawnX, -11.84 AS SpawnY UNION ALL
    SELECT 'valley', 0 AS PointId, 9.36 AS SpawnX, -14.22 AS SpawnY UNION ALL
    SELECT 'valley', 1 AS PointId, -1.87 AS SpawnX, -17.33 AS SpawnY UNION ALL
    SELECT 'valley', 2 AS PointId, -7.08 AS SpawnX, -24.84 AS SpawnY UNION ALL
    SELECT 'valley', 3 AS PointId, 4.45 AS SpawnX, -23.13 AS SpawnY
), FISH_SPAWNS AS (
    SELECT
        mr.MatchResultId,
        lvr.LevelResultId,
        lvr.LevelName,
        mr.LevelCount,
        ps.SpawnID AS SpawnId,
        ps.PlayerID AS PlayerId,
        ps.SpawnTime,
        ps.X AS SpawnX,
        ps.Y AS SpawnY
    FROM
        LevelResult lvr
    JOIN
        PlayerSpawn ps
        ON ps.SpawnTime BETWEEN lvr.StartTime AND lvr.EndTime
    JOIN
        MatchResult mr
        ON mr.MatchResultId = lvr.MatchResultId
    WHERE -- Throw away all but the spawn per player per level
        ps.SpawnID IN (
            SELECT MIN(ps2.SpawnID)
            FROM PlayerSpawn ps2
            JOIN LevelResult lvr2
                ON ps2.SpawnTime BETWEEN lvr2.StartTime AND lvr2.EndTime
            WHERE lvr2.LevelResultId = lvr.LevelResultId
            GROUP BY ps2.PlayerID
        )
), LEVEL_SPAWN_MATCHES AS (
    SELECT
        fs.MatchResultId,
        fs.LevelResultId,
        fs.LevelName,
        fs.LevelCount,
        fs.SpawnId,
        fs.SpawnTime,
        fs.SpawnX,
        fs.SpawnY,
        lsp.LevelName AS InferredLevelName,
        lsp.PointId AS InferredSpawnPoint,
        lsp.SpawnX AS InferredSpawnX,
        lsp.SpawnY AS InferredSpawnY,
        (((fs.SpawnX - lsp.SpawnX) * (fs.SpawnX - lsp.SpawnX)) + ((fs.SpawnY - lsp.SpawnY) * (fs.SpawnY - lsp.SpawnY))) AS Distance,
        CASE WHEN fs.LevelName = lsp.LevelName THEN 1 ELSE 0 END AS IsTrueLevel,
        CASE WHEN fs.LevelCount > 1 THEN 1 ELSE 0 END AS IsMultiLevelMatch
    FROM
        FISH_SPAWNS fs
    CROSS JOIN
        LEVEL_SPAWN_POINTS lsp
), BEST_LEVEL_POINT_DISTANCES AS (
    SELECT
        LevelResultId,
        SpawnId,
        InferredLevelName,
        MIN(Distance) AS MinDistance
    FROM
        LEVEL_SPAWN_MATCHES
    GROUP BY
        LevelResultId, SpawnId, InferredLevelName
), FULL_BEST_LEVEL_POINT_DISTANCES AS (
    SELECT
        lsm.MatchResultId,
        lsm.LevelResultId,
        lsm.LevelName,
        lsm.LevelCount,
        lsm.SpawnId,
        lsm.SpawnTime,
        lsm.SpawnX,
        lsm.SpawnY,
        lsm.InferredLevelName,
        lsm.InferredSpawnPoint,
        lsm.InferredSpawnX,
        lsm.InferredSpawnY,
        lsm.Distance,
        lsm.IsTrueLevel,
        lsm.IsMultiLevelMatch,
        CASE WHEN lsm.Distance = blpd.MinDistance THEN 1 ELSE 0 END AS IsBestPoint
    FROM
        LEVEL_SPAWN_MATCHES lsm
    JOIN
        BEST_LEVEL_POINT_DISTANCES blpd
        ON lsm.LevelResultId = blpd.LevelResultId
        AND lsm.SpawnId = blpd.SpawnId
        AND lsm.InferredLevelName = blpd.InferredLevelName
), INFERRED_LEVELS AS (
    SELECT
        LevelResultId,
        InferredLevelName,
        SUM(Distance) AS TotalDistance,
        AVG(Distance) AS AvgDistance,
        MIN(SUM(Distance)) OVER (PARTITION BY LevelResultId) AS MinTotalDistance
    FROM FULL_BEST_LEVEL_POINT_DISTANCES
    WHERE IsBestPoint = 1
    GROUP BY LevelResultId, InferredLevelName
), FULL_INFERRED_LEVELS AS (
    SELECT
        fblpd.MatchResultId,
        fblpd.LevelResultId,
        fblpd.LevelName,
        fblpd.LevelCount,
        fblpd.SpawnId,
        fblpd.SpawnTime,
        fblpd.SpawnX,
        fblpd.SpawnY,
        fblpd.InferredLevelName,
        fblpd.InferredSpawnPoint,
        fblpd.InferredSpawnX,
        fblpd.InferredSpawnY,
        fblpd.Distance,
        fblpd.IsTrueLevel,
        fblpd.IsMultiLevelMatch,
        fblpd.IsBestPoint,
        il.TotalDistance AS InferenceScore,
        il.AvgDistance AS InferenceAvgDistance,
        CASE WHEN il.TotalDistance = il.MinTotalDistance THEN 1 ELSE 0 END AS IsInferredLevel,
        CASE WHEN fblpd.IsTrueLevel = 1 AND il.TotalDistance = il.MinTotalDistance THEN 1 ELSE 0 END AS IsCorrectInference
    FROM
        FULL_BEST_LEVEL_POINT_DISTANCES fblpd
    LEFT JOIN
        INFERRED_LEVELS il
        ON fblpd.LevelResultId = il.LevelResultId
        AND fblpd.InferredLevelName = il.InferredLevelName
)

SELECT
    *
FROM
    FULL_INFERRED_LEVELS
