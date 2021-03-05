using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Tab : MonoBehaviour {

    [FormerlySerializedAs("tabType")] [SerializeField, Tooltip("The TabType to use")]
    private TabStyle _tabType;

    [FormerlySerializedAs("headerText")] [SerializeField, Tooltip("The name of the tab")]
    private string _headerText;

    //[SerializeField, Tooltip("Is the tab currently active?")]
    //private bool active;

    [FormerlySerializedAs("tabText")] [SerializeField, Tooltip("The GameObject which displays the header text of the tab")]
    private Text _tabText;

    [FormerlySerializedAs("tabButton")] [SerializeField, Tooltip("The GameObject which displays the header text of the tab")]
    private Button _tabButton;

    [FormerlySerializedAs("scrollView")] [SerializeField, Tooltip("The GameObject which holds the content of the tab")]
    private ScrollRect _scrollView;


    // Use this for initialization
    void Awake() {

        if( _tabButton ) {
            if( _tabText ) {
                _tabText.text = _headerText;

                // Change width according to the number of letters and keep height
                //Vector2 newTextSize = new Vector2(tabType.pixelWidthPerLetter * headerText.Length, tabText.GetComponent<RectTransform>().sizeDelta.y);
                Vector2 newTextSize = new Vector2(_tabText.preferredWidth, _tabText.GetComponent<RectTransform>().rect.height);
                _tabText.GetComponent<RectTransform>().sizeDelta = newTextSize;

                // Set the button size according to the text size
                Vector2 oldSize = _tabText.GetComponent<RectTransform>().sizeDelta;
                //float newWidth = oldSize.x + tabType.innerTabXPadding;
                float newWidth = _tabText.preferredWidth + _tabType.InnerTabXPadding;
                Vector2 newSize = new Vector2(newWidth, oldSize.y + _tabType.InnerTabYPadding);
                _tabButton.GetComponent<RectTransform>().sizeDelta = newSize;
            } else {
                Debug.LogError(name + " has no tabText field set in the inspector");
            }

            // Set the Button Event Listener so that this Tab becomes active if the tab button is pressed
            _tabButton.onClick.AddListener(SetSelfAsActiveTab);

        } else {
            Debug.LogError(name + " has no tabButton field set in the inspector");
        }

        // To identify the tabs in the hierarchy while the scene is running, just quality of life
        transform.name = "Tab" + " (" + _headerText + ")";

    }

    void SetSelfAsActiveTab() {
        GetComponentInParent<TabController>().SetActiveTab(this);
    }

    public void SetButtonXPosition(float xPos) {
        Vector2 oldPos = _tabButton.GetComponent<RectTransform>().anchoredPosition;
        Vector2 newPos = new Vector2(xPos, oldPos.y);
        _tabButton.GetComponent<RectTransform>().anchoredPosition = newPos;
    }

    public float GetRightCornerPos() {
        RectTransform rt = _tabButton.GetComponent<RectTransform>();
        float rightSidePos = rt.rect.width + rt.anchoredPosition.x;
        return rightSidePos;
    }

    public void SetActiveState(bool active) {
        if( active ) {
            _tabText.material = _tabType.ActiveMaterial;
            _tabButton.GetComponent<Image>().sprite = _tabType.ActiveImage;
        } else {
            _tabText.material = _tabType.InactiveMaterial;
            _tabButton.GetComponent<Image>().sprite = _tabType.InactiveImage;
        }
        _scrollView.gameObject.SetActive(active);
    }
}
