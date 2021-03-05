using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SCTEST : MonoBehaviour
{
  

    private static string[] FindAllMaterial()
    {
        var guids = AssetDatabase.FindAssets("t:Material");
        List<string> paths = new List<string>();
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            paths.Add(path);
        }
        return paths.ToArray();
    }

    // Start is called before the first frame update
    [MenuItem("Test/Run")]
    public static void Open()
    {
        List<string> textureNames = new List<string>();
        List<Texture> textures = new List<Texture>();

        var paths = FindAllMaterial();

        foreach (var path in paths)
        {
            Material material = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
            if (material != null)
            {
                string[] names = material.GetTexturePropertyNames();
            
                foreach (string name in names)
                {
                    if (name.Contains("Metallic"))
                    {
                        textureNames.Add(name);
                        if(material.GetTexture(name) != null)
                            textures.Add(material.GetTexture(name));
                        
                    }
                }
                

            }
        }

        foreach (var texture in textures)
        {
           if(texture.graphicsFormat == UnityEngine.Experimental.Rendering.GraphicsFormat.RGB_ETC2_SRGB)
                Debug.Log(texture.name);
            
        }
    }
}
