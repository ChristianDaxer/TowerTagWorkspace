using UnityEngine;

public static class ColorRampToTexture {
    public static Texture2D ConvertColorRampToTexture(int width, int height, TextureFormat format, Gradient colorRamp,
        FilterMode filterMode) {
        var tex = new Texture2D(width, height, format, true, true);

        for (var x = 0; x < width; x++) {
            Color c = colorRamp.Evaluate((float) x / width);
            for (var y = 0; y < height; y++) {
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        tex.filterMode = filterMode;
        return tex;
    }
}