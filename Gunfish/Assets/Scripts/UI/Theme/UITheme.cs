using TMPro;
using UnityEngine;

public static class UITheme {
    public readonly struct TypeStyle {
        public readonly TMP_FontAsset Font;
        public readonly float Size;
        public readonly FontStyles Style;

        public TypeStyle(string fontResourcePath, float size, FontStyles style = FontStyles.Normal) {
            Font = Resources.Load<TMP_FontAsset>(fontResourcePath);
            Size = size;
            Style = style;
        }
    }

    public static readonly TypeStyle PageTitle =
        new TypeStyle("Fonts/Oswald/static/Oswald-Bold SDF", 64f, FontStyles.Bold);
    public static readonly TypeStyle SectionHeader =
        new TypeStyle("Fonts/Oswald/static/Oswald-SemiBold SDF", 32f);
    public static readonly TypeStyle Body =
        new TypeStyle("Fonts/Oswald/static/Oswald-Regular SDF", 24f);
    public static readonly TypeStyle ButtonLabel =
        new TypeStyle("Fonts/Oswald/static/Oswald-Medium SDF", 20f);

    public static readonly Color Surface900 = new Color32(0x05, 0x0F, 0x1C, 0xFF);
    public static readonly Color Surface700 = new Color32(0x0A, 0x27, 0x40, 0xFF);
    public static readonly Color Surface500 = new Color32(0x14, 0x3D, 0x5C, 0xFF);
}
