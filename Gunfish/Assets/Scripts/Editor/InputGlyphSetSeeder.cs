using System.IO;
using UnityEditor;
using UnityEngine;

public static class InputGlyphSetSeeder {
    private const string KenneyRoot = "Assets/Resources/Sprites/UI/kenney_input_prompts";
    private const string OutputRoot = "Assets/Resources/ScriptableObjects/InputGlyphs";

    // The fixed semantic action set. Move represents the entire 2-axis movement cluster
    // (WASD/arrows on keyboard, left analog stick on gamepad, the physical joystick on
    // cabinet) as a single composite glyph per scheme — a 2-axis control can't be reduced
    // to "one control's bare name" the way a single button binding can.
    private static readonly (PlayerAction action, string kenneyFile)[] KeyboardEntries = {
        (PlayerAction.Select, "keyboard_enter.png"),
        (PlayerAction.Back, "keyboard_backspace.png"),
        (PlayerAction.Menu, "keyboard_escape.png"),
        (PlayerAction.Move, "keyboard_arrows_all.png"),
        (PlayerAction.Fire, "keyboard_space.png"),
    };

    private static readonly (PlayerAction action, string kenneyFile)[] GamepadEntries = {
        (PlayerAction.Select, "xbox_button_a.png"),
        (PlayerAction.Back, "xbox_button_b.png"),
        (PlayerAction.Menu, "xbox_button_start.png"),
        (PlayerAction.Move, "xbox_stick_l.png"),
        (PlayerAction.Fire, "xbox_rt.png"),
    };

    // No Menu entry: the cabinet has no physical Menu button. InputGlyphDatabase.GetSprite
    // special-cases (Joystick, Menu) to explicitly fall back to the KEYBOARD Menu glyph —
    // see that method; do not add a Menu row here. Select and Fire intentionally share the
    // same sprite: the cabinet's single physical front-panel button is dual-purpose
    // (confirm/shoot).
    private static readonly (PlayerAction action, string kenneyFile)[] CabinetEntries = {
        (PlayerAction.Select, "generic_button_trigger_a.png"),
        (PlayerAction.Back, "generic_button_trigger_b.png"),
        (PlayerAction.Move, "generic_joystick_red.png"),
        (PlayerAction.Fire, "generic_button_trigger_a.png"),
    };

    [MenuItem("Tools/Gunfish/Seed Input Glyph Sets")]
    public static void Seed() {
        Directory.CreateDirectory(OutputRoot);

        var keyboardSet = SeedSet("KeyboardGlyphs", $"{KenneyRoot}/Keyboard & Mouse/Default", KeyboardEntries);
        var gamepadSet = SeedSet("GamepadGlyphs", $"{KenneyRoot}/Xbox Series/Default", GamepadEntries);
        var cabinetSet = SeedSet("CabinetGlyphs", $"{KenneyRoot}/Generic/Default", CabinetEntries);
        var fallback = LoadGlyphSprite($"{KenneyRoot}/Generic/Default/generic_button.png");

        var database = LoadOrCreateAsset<InputGlyphDatabase>($"{OutputRoot}/InputGlyphDatabase.asset");
        database.keyboardGlyphs = keyboardSet;
        database.gamepadGlyphs = gamepadSet;
        database.cabinetGlyphs = cabinetSet;
        database.fallbackGlyph = fallback;
        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();
        Debug.Log("Seeded Input Glyph Sets: " +
            $"{keyboardSet.entries.Count} keyboard, {gamepadSet.entries.Count} gamepad, {cabinetSet.entries.Count} cabinet entries.");
    }

    private static InputGlyphSet SeedSet(string assetName, string sourceFolder, (PlayerAction action, string kenneyFile)[] entries) {
        var set = LoadOrCreateAsset<InputGlyphSet>($"{OutputRoot}/{assetName}.asset");
        set.entries.Clear();
        foreach (var (action, kenneyFile) in entries) {
            var sprite = LoadGlyphSprite($"{sourceFolder}/{kenneyFile}");
            set.entries.Add(new InputGlyphSet.Entry { action = action, glyph = sprite });
        }
        EditorUtility.SetDirty(set);
        return set;
    }

    private static Sprite LoadGlyphSprite(string path) {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) {
            if (asset is Sprite sprite) return sprite;
        }
        Debug.LogError($"[InputGlyphSetSeeder] No sprite sub-asset found at {path}");
        return null;
    }

    private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
