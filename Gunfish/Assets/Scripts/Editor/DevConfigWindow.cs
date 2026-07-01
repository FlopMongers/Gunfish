using UnityEditor;
using UnityEngine;

public class DevConfigWindow : EditorWindow {
    [MenuItem("Tools/Gunfish/Dev Config")]
    public static void ShowWindow() {
        GetWindow<DevConfigWindow>("Dev Config");
    }

    private void OnGUI() {
        EditorGUILayout.HelpBox(
            "Overrides apply only in the Editor (stored in EditorPrefs, per-machine). " +
            "They never affect builds and never touch any committed file.",
            MessageType.Info);

        EditorGUILayout.Space();
        var enabledLabel = new GUIContent("Enable Dev Overrides", "Enables the use of development overrides for GameManager settings.");
        DevConfigOverride.Enabled = EditorGUILayout.Toggle(enabledLabel, DevConfigOverride.Enabled);

        using (new EditorGUI.DisabledScope(!DevConfigOverride.Enabled)) {
            EditorGUILayout.Space();

            var gameModeListLabel = new GUIContent("Game Mode List", "Overrides the GameModeList used by the GameManager.");
            DevConfigOverride.GameModeListOverride = (GameModeList)EditorGUILayout.ObjectField(
                gameModeListLabel, DevConfigOverride.GameModeListOverride, typeof(GameModeList), false);

            var gunfishDataListLabel = new GUIContent("Gunfish Data List", "Overrides the GunfishDataList used by the GameManager.");
            DevConfigOverride.GunfishDataListOverride = (GunfishDataList)EditorGUILayout.ObjectField(
                gunfishDataListLabel, DevConfigOverride.GunfishDataListOverride, typeof(GunfishDataList), false);

            EditorGUILayout.Space();
            var debugLabel = new GUIContent("Debug", "Enables debug mode, which allows for simulating multiple players and other debug features.");
            DevConfigOverride.DebugOverride = EditorGUILayout.Toggle(debugLabel, DevConfigOverride.DebugOverride);

            var playerCountLabel = new GUIContent("Debug Player Count", "Number of players to simulate in debug mode. Must be between 2 and 4.");
            DevConfigOverride.DebugPlayerCountOverride = EditorGUILayout.IntSlider(
                playerCountLabel, DevConfigOverride.DebugPlayerCountOverride, 1, 4);
        }
    }
}
