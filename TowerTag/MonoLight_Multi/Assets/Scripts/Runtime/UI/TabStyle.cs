using UnityEngine;

[CreateAssetMenu(menuName = "UI Elements/TabType")]
public class TabStyle : ScriptableObject {

    [SerializeField, Tooltip("The material to use when the tab is active")]
    private Material _activeMaterial;
    public Material ActiveMaterial {
        get {
            return _activeMaterial;
        }
    }

    [SerializeField, Tooltip("The material to use when the tab is inactive")]
    private Material _inactiveMaterial;
    public Material InactiveMaterial {
        get {
            return _inactiveMaterial;
        }
    }

    [SerializeField, Tooltip("The sprite to display when the tab is active")]
    private Sprite _activeImage;
    public Sprite ActiveImage {
        get {
            return _activeImage;
        }
    }

    [SerializeField, Tooltip("The sprite to display when the tab is inactive")]
    private Sprite _inactiveImage;
    public Sprite InactiveImage {
        get {
            return _inactiveImage;
        }
    }


    [SerializeField, Tooltip("The amount of pixel padding on the x axis between the text and the button")]
    private float _innerTabXPadding = 20;
    public float InnerTabXPadding {
        get {
            return _innerTabXPadding;
        }
    }

    [SerializeField, Tooltip("The amount of pixel padding on the y axis between the text and the button")]
    private float _innerTabYPadding = 20;
    public float InnerTabYPadding {
        get {
            return _innerTabYPadding;
        }
    }
}