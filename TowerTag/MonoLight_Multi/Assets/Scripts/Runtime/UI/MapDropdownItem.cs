using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapDropdownItem : MonoBehaviour {
    [SerializeField] private Material _inactiveMaterial;

    private void Start () {
        var adminController = GetComponentInParent<MatchManager>();
        var textField = GetComponentInChildren<TMP_Text>();
        bool selectable = adminController.IsMapSelectable(textField.text);
        GetComponent<Toggle>().interactable = selectable;
        if (!selectable)
            textField.color = _inactiveMaterial.color;
    }
}
