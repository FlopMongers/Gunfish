using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Glyph Database", menuName = "Scriptable Objects/Input Glyph Database")]
public class InputGlyphDatabase : ScriptableObject {
    public InputGlyphSet keyboardGlyphs;
    public InputGlyphSet gamepadGlyphs;
    public InputGlyphSet cabinetGlyphs;
    public Sprite fallbackGlyph;

    public Sprite GetSprite(InputAction action, string controlScheme) {
        InputGlyphSet set = controlScheme switch {
            "Keyboard&Mouse" => keyboardGlyphs,
            "Gamepad" => gamepadGlyphs,
            "Joystick" => cabinetGlyphs,
            _ => null,
        };
        if (set == null) return fallbackGlyph;

        foreach (var binding in action.bindings) {
            if (binding.isComposite || binding.isPartOfComposite) continue;
            if (!string.IsNullOrEmpty(binding.groups) && !binding.groups.Split(';').Contains(controlScheme))
                continue;

            var path = binding.effectivePath;
            var controlName = path.Substring(path.LastIndexOf('/') + 1);
            if (set.TryGetSprite(controlName, out var sprite))
                return sprite;
        }
        return fallbackGlyph;
    }
}
