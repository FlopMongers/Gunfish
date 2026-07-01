#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DevConfigOverride {
    private const string EnabledKey = "Gunfish.Dev.Enabled";
    private const string GameModeListGuidKey = "Gunfish.Dev.GameModeListGuid";
    private const string GunfishDataListGuidKey = "Gunfish.Dev.GunfishDataListGuid";
    private const string DebugKey = "Gunfish.Dev.Debug";
    private const string DebugPlayerCountKey = "Gunfish.Dev.DebugPlayerCount";

    public static bool Enabled {
        get => EditorPrefs.GetBool(EnabledKey, false);
        set => EditorPrefs.SetBool(EnabledKey, value);
    }

    public static GameModeList GameModeListOverride {
        get => LoadAsset<GameModeList>(GameModeListGuidKey);
        set => SaveAsset(GameModeListGuidKey, value);
    }

    public static GunfishDataList GunfishDataListOverride {
        get => LoadAsset<GunfishDataList>(GunfishDataListGuidKey);
        set => SaveAsset(GunfishDataListGuidKey, value);
    }

    public static bool DebugOverride {
        get => EditorPrefs.GetBool(DebugKey, false);
        set => EditorPrefs.SetBool(DebugKey, value);
    }

    public static int DebugPlayerCountOverride {
        get => EditorPrefs.GetInt(DebugPlayerCountKey, 1);
        set => EditorPrefs.SetInt(DebugPlayerCountKey, value);
    }

    public static bool TryGetGameModeList(out GameModeList gameModeList) {
        gameModeList = Enabled ? GameModeListOverride : null;
        return gameModeList != null;
    }

    public static bool TryGetGunfishDataList(out GunfishDataList gunfishDataList) {
        gunfishDataList = Enabled ? GunfishDataListOverride : null;
        return gunfishDataList != null;
    }

    public static bool TryGetDebug(out bool debug) {
        debug = DebugOverride;
        return Enabled;
    }

    public static bool TryGetDebugPlayerCount(out int debugPlayerCount) {
        debugPlayerCount = DebugPlayerCountOverride;
        return Enabled;
    }

    private static T LoadAsset<T>(string guidKey) where T : Object {
        var guid = EditorPrefs.GetString(guidKey, string.Empty);
        if (string.IsNullOrEmpty(guid)) {
            return null;
        }
        var path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static void SaveAsset(string guidKey, Object asset) {
        if (asset == null) {
            EditorPrefs.SetString(guidKey, string.Empty);
            return;
        }
        var path = AssetDatabase.GetAssetPath(asset);
        var guid = AssetDatabase.AssetPathToGUID(path);
        EditorPrefs.SetString(guidKey, guid);
    }
}
#endif
