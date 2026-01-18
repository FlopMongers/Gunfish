/*
Script to create analytic database for GunFish stats from transactional game logs.
*/
BEGIN TRANSACTION;
ALTER TABLE MatchResult RENAME TO RawMatchResult;
ALTER TABLE PlayerMatchResult RENAME TO RawPlayerMatchResult;
ALTER TABLE LevelResult RENAME TO RawLevelResult;
ALTER TABLE PlayerLevelResult RENAME TO RawPlayerLevelResult;
ALTER TABLE PlayerDeath RENAME TO RawPlayerDeath;
ALTER TABLE PlayerSpawn RENAME TO RawPlayerSpawn;


CREATE TABLE LEVEL_SPAWN_POINTS AS (
    SELECT 'acid_factory' AS LevelName, 0 AS PointId, -23.63516 AS SpawnX, -8.419025 AS SpawnY UNION ALL
    SELECT 'acid_factory', 1 AS PointId, 5.274841 AS SpawnX, -8.399027 AS SpawnY UNION ALL
    SELECT 'acid_factory', 2 AS PointId, -15.52516 AS SpawnX, -8.449028 AS SpawnY UNION ALL
    SELECT 'acid_factory', 3 AS PointId, -4.605159 AS SpawnX, -8.419025 AS SpawnY UNION ALL
    SELECT 'barrel', 0 AS PointId, -6.3 AS SpawnX, 0.9 AS SpawnY UNION ALL
    SELECT 'barrel', 1 AS PointId, 3.7 AS SpawnX, 2.2 AS SpawnY UNION ALL
    SELECT 'barrel', 2 AS PointId, 1.6 AS SpawnX, -9.1 AS SpawnY UNION ALL
    SELECT 'barrel', 3 AS PointId, -4.78 AS SpawnX, -9.1 AS SpawnY UNION ALL
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
);

CREATE TABLE MatchResult (
    MatchID INTEGER PRIMARY KEY,
    GameMode TEXT,
    StartTime DATETIME,
    EndTime DATETIME,
    LevelCount INTEGER,
    PlayerCount INTEGER
);

CREATE TABLE PlayerMatchResult (
    PlayerMatchID INTEGER PRIMARY KEY,
    MatchID INTEGER,
    PlayerID INTEGER,
    TeamID INTEGER,
    Score INTEGER,
    Kills INTEGER,
    Deaths INTEGER,
    FOREIGN KEY (MatchID) REFERENCES MatchResult(MatchID)
    FOREIGN KEY (PlayerID) REFERENCES Entities(EntityID)
);

CREATE TABLE LevelResult (
    LevelID INTEGER PRIMARY KEY,
    MatchID INTEGER,
    LevelName TEXT,
    StartTime DATETIME,
    EndTime DATETIME,
    FOREIGN KEY (MatchID) REFERENCES MatchResult(MatchID)
);

CREATE TABLE PlayerLevelResult (
    PlayerLevelID INTEGER PRIMARY KEY,
    LevelID INTEGER,
    PlayerID INTEGER,
    Score INTEGER,
    Kills INTEGER,
    Deaths INTEGER,
    FOREIGN KEY (LevelID) REFERENCES LevelResult(LevelID)
    FOREIGN KEY (PlayerID) REFERENCES Entities(EntityID)
);

CREATE TABLE PlayerDeath (
    DeathID INTEGER PRIMARY KEY,
    PlayerID INTEGER,
    LevelID INTEGER,
    DeathTime DATETIME,
    KillerID INTEGER,
    WeaponUsed TEXT,
    X INTEGER,
    Y INTEGER,
    FOREIGN KEY (PlayerID) REFERENCES Entities(EntityID)
    FOREIGN KEY (LevelID) REFERENCES LevelResult(LevelID)
    FOREIGN KEY (KillerID) REFERENCES Entities(EntityID)
);

CREATE TABLE PlayerSpawn (
    SpawnID INTEGER PRIMARY KEY,
    PlayerID INTEGER,
    LevelID INTEGER,
    SpawnTime DATETIME,
    X INTEGER,
    Y INTEGER,
    FOREIGN KEY (PlayerID) REFERENCES Entities(EntityID)
    FOREIGN KEY (LevelID) REFERENCES LevelResult(LevelID)
);
