using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fast way to load the custom Image by SpriteType if customization is allowed
/// </summary>
[RequireComponent(typeof(Image))]
public class CustomSpriteLoader : MonoBehaviour {
    [SerializeField] private Customization.SpriteType _spriteType;
    private Image _image;

    private Customization.SpriteType SpriteType => _spriteType;

    private void Awake() {
        if (!Customization.UseCustomLogos) {
            enabled = false;
            return;
        }

        _image = GetComponent<Image>();
        var sprite = Customization.GetSpriteByType(SpriteType);
        if (sprite != null) {
            _image.sprite = sprite;
            _image.sprite.texture.Apply(true);
        }
        else {
            Debug.Log("Customization enabled but no fitting sprite found");
        }
    }
}
