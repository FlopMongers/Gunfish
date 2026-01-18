-- Associate powerup pickups with level results
SELECT
    pup.PickupId,
    pup.PlayerId,
    pup.PickupTime,
    pup.PowerUpType,
    lvr.LevelResultId,
    lvr.MatchResultId
FROM
    PowerUpPickup pup
LEFT JOIN
    LevelResult lvr
    ON pup.PickupTime BETWEEN lvr.StartTime AND lvr.EndTime
