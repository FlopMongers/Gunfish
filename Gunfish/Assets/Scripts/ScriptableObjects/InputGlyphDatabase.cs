using UnityEngine;

[CreateAssetMenu(fileName = "New Input Glyph Database", menuName = "Scriptable Objects/Input Glyph Database")]
public class InputGlyphDatabase : ScriptableObject {
    public InputGlyphSet keyboardGlyphs;
    public InputGlyphSet gamepadGlyphs;
    public InputGlyphSet cabinetGlyphs;
    public Sprite fallbackGlyph;

    public Sprite GetSprite(PlayerAction action, string controlScheme) {
        InputGlyphSet set;

        if (PlatformConfig.IsCabinet) {
            // Cabinet builds only ever have two real input sources: the cabinet's own
            // controller, and -- as an Editor/debug-only fallback (e.g. testing with the
            // simulated ArcadeCabinet build target while using a real keyboard) -- a
            // real keyboard. We deliberately do NOT gate on the literal string
            // "Joystick": the cabinet's physical joystick/buttons may enumerate to the
            // Input System as a generic gamepad-like device and report "Gamepad"
            // instead of "Joystick", so on cabinet hardware "not Keyboard&Mouse" means
            // "the cabinet," full stop -- matching by scheme-name string alone is not
            // trustworthy here.
            if (controlScheme == "Keyboard&Mouse") {
                set = keyboardGlyphs;
            } else {
                // Explicit override: the cabinet's physical front panel has no "Menu"
                // button. Per design, Menu on cabinet always shows the KEYBOARD glyph
                // (Esc) -- never the generic fallback sprite. cabinetGlyphs
                // deliberately has no Menu entry seeded (see
                // InputGlyphSetSeeder.CabinetEntries) so this branch is the only path
                // that can ever produce a cabinet-scheme Menu sprite.
                if (action == PlayerAction.Menu) {
                    if (keyboardGlyphs != null && keyboardGlyphs.TryGetSprite(PlayerAction.Menu, out var keyboardMenuSprite))
                        return keyboardMenuSprite;
                    return fallbackGlyph;
                }
                set = cabinetGlyphs;
            }
        } else {
            // Off cabinet, only a real keyboard or a real gamepad are meaningful
            // control schemes. cabinetGlyphs is never relevant on non-cabinet hardware,
            // so an unrecognized scheme (including a literal "Joystick", which is dead
            // off cabinet -- see design notes) falls through to the generic fallback.
            set = controlScheme switch {
                "Keyboard&Mouse" => keyboardGlyphs,
                "Gamepad" => gamepadGlyphs,
                _ => null,
            };
        }

        if (set == null) return fallbackGlyph;
        return set.TryGetSprite(action, out var sprite) ? sprite : fallbackGlyph;
    }
}
