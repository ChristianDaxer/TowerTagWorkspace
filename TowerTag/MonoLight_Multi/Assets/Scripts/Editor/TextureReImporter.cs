using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

class TextureReImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        try
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            TextureImporterPlatformSettings currentSettings = textureImporter.GetPlatformTextureSettings("Android");
            if (currentSettings.overridden == true)
            {
                TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
                settings = currentSettings;
                settings.format = TextureImporterFormat.ASTC_8x8;
                textureImporter.SetPlatformTextureSettings(settings);
                Debug.Log("Reimported Texture : " + textureImporter.assetPath);
                textureImporter.SaveAndReimport();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not change format setting for this texture : " + e);
        }
    }
}
