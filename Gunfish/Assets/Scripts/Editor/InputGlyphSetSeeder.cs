using System.IO;
using UnityEditor;
using UnityEngine;

public static class InputGlyphSetSeeder {
    private const string KenneyRoot = "Assets/Resources/Sprites/UI/kenney_input_prompts";
    private const string OutputRoot = "Assets/Resources/ScriptableObjects/InputGlyphs";

    private static readonly (string controlName, string kenneyFile)[] KeyboardEntries = {
        ("a", "keyboard_a.png"), ("b", "keyboard_b.png"), ("c", "keyboard_c.png"),
        ("d", "keyboard_d.png"), ("e", "keyboard_e.png"), ("f", "keyboard_f.png"),
        ("g", "keyboard_g.png"), ("h", "keyboard_h.png"), ("i", "keyboard_i.png"),
        ("j", "keyboard_j.png"), ("k", "keyboard_k.png"), ("l", "keyboard_l.png"),
        ("m", "keyboard_m.png"), ("n", "keyboard_n.png"), ("o", "keyboard_o.png"),
        ("p", "keyboard_p.png"), ("q", "keyboard_q.png"), ("r", "keyboard_r.png"),
        ("s", "keyboard_s.png"), ("t", "keyboard_t.png"), ("u", "keyboard_u.png"),
        ("v", "keyboard_v.png"), ("w", "keyboard_w.png"), ("x", "keyboard_x.png"),
        ("y", "keyboard_y.png"), ("z", "keyboard_z.png"),
        ("0", "keyboard_0.png"), ("1", "keyboard_1.png"), ("2", "keyboard_2.png"),
        ("3", "keyboard_3.png"), ("4", "keyboard_4.png"), ("5", "keyboard_5.png"),
        ("6", "keyboard_6.png"), ("7", "keyboard_7.png"), ("8", "keyboard_8.png"),
        ("9", "keyboard_9.png"),
        ("space", "keyboard_space.png"),
        ("leftShift", "keyboard_shift.png"), ("rightShift", "keyboard_shift.png"),
        ("leftCtrl", "keyboard_ctrl.png"), ("rightCtrl", "keyboard_ctrl.png"),
        ("leftAlt", "keyboard_alt.png"), ("rightAlt", "keyboard_alt.png"),
        ("enter", "keyboard_enter.png"),
        ("escape", "keyboard_escape.png"),
        ("tab", "keyboard_tab.png"),
        ("upArrow", "keyboard_arrow_up.png"), ("downArrow", "keyboard_arrow_down.png"),
        ("leftArrow", "keyboard_arrow_left.png"), ("rightArrow", "keyboard_arrow_right.png"),
    };

    private static readonly (string controlName, string kenneyFile)[] GamepadEntries = {
        ("buttonSouth", "xbox_button_a.png"),
        ("buttonEast", "xbox_button_b.png"),
        ("buttonWest", "xbox_button_x.png"),
        ("buttonNorth", "xbox_button_y.png"),
        ("leftShoulder", "xbox_lb.png"),
        ("rightShoulder", "xbox_rb.png"),
        ("leftTrigger", "xbox_lt.png"),
        ("rightTrigger", "xbox_rt.png"),
        ("start", "xbox_button_start.png"),
        ("select", "xbox_button_back.png"),
        ("leftStickPress", "xbox_stick_l_press.png"),
        ("rightStickPress", "xbox_stick_r_press.png"),
    };

    private static readonly (string controlName, string kenneyFile)[] CabinetEntries = {
        ("trigger", "generic_button_trigger_a.png"),
        ("stick", "generic_joystick_red.png"),
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

    private static InputGlyphSet SeedSet(string assetName, string sourceFolder, (string controlName, string kenneyFile)[] entries) {
        var set = LoadOrCreateAsset<InputGlyphSet>($"{OutputRoot}/{assetName}.asset");
        set.entries.Clear();
        foreach (var (controlName, kenneyFile) in entries) {
            var sprite = LoadGlyphSprite($"{sourceFolder}/{kenneyFile}");
            set.entries.Add(new InputGlyphSet.Entry { controlName = controlName, glyph = sprite });
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
