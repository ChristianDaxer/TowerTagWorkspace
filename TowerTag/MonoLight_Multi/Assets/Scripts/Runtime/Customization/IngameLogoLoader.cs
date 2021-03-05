using UnityEngine;
using UnityEngine.UI;
using static Customization;

public class IngameLogoLoader : MonoBehaviour {

    [SerializeField, Tooltip("The Canvas for the Images")]
    private Canvas _canvas;
    [SerializeField, Tooltip("Prefab that gets spawned when images found")]
    private GameObject _emptyLogoPrefab;

    //Remove these when the reworked team manager is merged!
    [SerializeField] private Material _iceMaterial;
    [SerializeField] private Material _fireMaterial;

    private Sprite _logo1;
    private Sprite _logo2;
    private Sprite _logo;

    private void Start() {
        if (IsTwoColorLogoValid()) {
            CreateImageAsChild(_logo1, _iceMaterial);
            CreateImageAsChild(_logo2, _fireMaterial);

        } else if (IsSingleLogoValid()) {
            CreateImageAsChild(_logo, _iceMaterial);
        }
        else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Looks for a fitting image for the MapLogo SpriteType
    /// </summary>
    /// <returns>true when a fitting sprite is found</returns>
    private bool IsSingleLogoValid() {
        _logo = GetSpriteByType(SpriteType.MapLogo);
        return _logo != null && IsSizeValid(_logo);
    }

    /// <summary>
    /// Looks for a fitting image for the MapLogo_1 and MapLogo_2 SpriteType
    /// </summary>
    /// <returns>true when all fitting sprites are found</returns>
    private bool IsTwoColorLogoValid() {
        _logo1 = GetSpriteByType(SpriteType.MapLogo1);
        _logo2 = GetSpriteByType(SpriteType.MapLogo2);
        return _logo1 != null && _logo2 != null && IsSizeValid(_logo1) && IsSizeValid(_logo2);
    }

    private bool IsSizeValid(Sprite sprite) {
        return sprite.texture.width == 2048 && sprite.texture.height == 2048;
    }

    /// <summary>
    /// Spawns a GameObject with an Image as Component and adds the sprite on it
    /// Additionally it applies a material when given
    /// </summary>
    /// <param name="sprite">Sprite gets applied on the image component</param>
    /// <param name="material">Material for the Image component</param>
    private void CreateImageAsChild(Sprite sprite,  Material material = null) {
        GameObject logo = InstantiateWrapper.InstantiateWithMessage(_emptyLogoPrefab,_canvas.transform);
        var img = logo.GetComponent<Image>();
        img.sprite = sprite;
        img.sprite.texture.Apply(true);
        if (material != null) {
            img.material = material;
        }
    }
}