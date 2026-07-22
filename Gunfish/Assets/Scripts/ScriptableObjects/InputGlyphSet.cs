using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Input Glyph Set", menuName = "Scriptable Objects/Input Glyph Set")]
public class InputGlyphSet : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public PlayerAction action;
        public Sprite glyph;
    }

    public List<Entry> entries = new List<Entry>();

    public bool TryGetSprite(PlayerAction action, out Sprite sprite) {
        foreach (var entry in entries) {
            if (entry.action == action) {
                sprite = entry.glyph;
                return true;
            }
        }
        sprite = null;
        return false;
    }
}
