using System;
using System.Collections.Generic;
using UnityEngine;

public static class AssetsDAO {
    private static GunfishData[] gunfishData;
    private static GameMode[] gameModeData;

    private static T[] LoadScriptableObjects<T>() where T : ScriptableObject {
        var type = typeof(T);
        return Resources.LoadAll<T>("");
    }

    public static GunfishData[] LoadGunfishData() {
        if (gunfishData == null) {
            gunfishData = LoadScriptableObjects<GunfishData>();
        }
        return gunfishData;
    }

    public static GameMode[] LoadGameModeData() {
        if (gameModeData == null) {
            gameModeData = LoadScriptableObjects<GameMode>();
        }
        return gameModeData;
    }
}
