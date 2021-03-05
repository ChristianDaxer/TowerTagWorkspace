using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Every "drag and drop" item must contain this script
/// </summary>
[RequireComponent(typeof(Image))]
public class DragAndDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public static DragAndDropItem DraggedItem;                                      // Item that is dragged now
    public static GameObject Icon;                                                  // Icon of dragged item
    public static DragAndDropCell SourceCell;                                       // From this cell dragged item is

    public delegate void DragEvent(DragAndDropItem item);
    public static event DragEvent OnItemDragStartEvent;                             // Drag start event
    public static event DragEvent OnItemDragEndEvent;                               // Drag end event

    [SerializeField]
    private Image iconPrefab;

    /// <summary>
    /// This item is dragged
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData) {
        // TODO: Hide the current Object by disabling the InputField, FocusButton and EditButton?

        var canvas = GetComponentInParent<Canvas>();                             // Get parent canvas
        SourceCell = GetComponentInParent<DragAndDropCell>();                       // Remember source cell
        DraggedItem = this;                                                         // Set as dragged item

        Icon = InstantiateWrapper.InstantiateWithMessage(iconPrefab.gameObject, canvas.transform);
        Icon.transform.SetAsLastSibling();                                      // Set as last child in canvas transform

        Vector2 sizeCell = SourceCell.GetComponent<RectTransform>().sizeDelta;      //Set the icon to the size of the cells
        Icon.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeCell.x, sizeCell.y);

        // Set image settings
        var image = Icon.GetComponent<Image>();
        image.raycastTarget = false;                                                // Disable icon's raycast for correct drop handling
        //image.sprite = GetComponent<Image>().sprite;

        // Copy the players text
        var text = Icon.GetComponentInChildren<TMP_Text>();
        text.raycastTarget = false;
        text.text = GetComponent<PlayerLineController>().Player.PlayerName;

        OnItemDragStartEvent?.Invoke(this);                                             // Notify all about item drag start
    }

    /// <summary>
    /// Every frame on this item drag
    /// </summary>
    /// <param name="data"></param>
    public void OnDrag(PointerEventData data) {
        if (Icon != null) {
            Icon.transform.position = Input.mousePosition;                          // Item's icon follows to cursor
        }
    }

    /// <summary>
    /// This item is dropped
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData) {
        if (Icon != null) {
            Destroy(Icon);                                                          // Destroy icon on item drop
        }
        MakeVisible(true);                                                          // Make item visible in cell
        OnItemDragEndEvent?.Invoke(this);                                               // Notify all cells about item drag end
        DraggedItem = null;
        Icon = null;
        SourceCell = null;
    }

    /// <summary>
    /// Enable item's raycast
    /// </summary>
    /// <param name="condition"> true - enable, false - disable </param>
    public void MakeRaycast(bool condition) {
        Image image = GetComponent<Image>();
        if (image != null) {
            image.raycastTarget = condition;
        }
    }

    /// <summary>
    /// Enable item's visibility
    /// </summary>
    /// <param name="condition"> true - enable, false - disable </param>
    public void MakeVisible(bool condition) {
        GetComponent<Image>().enabled = condition;
    }
}
