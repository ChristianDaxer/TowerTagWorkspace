using TowerTagSOES;
using UnityEngine;

public class TabController : MonoBehaviour {

    [SerializeField, Tooltip("The tabs which should be displayed for the player in the order they should be displayed")]
    private Tab[] _enabledPlayerTabs;

    [SerializeField, Tooltip("The tabs which should be displayed for the admin in the order they should be displayed")]
    private Tab[] _enabledAdminTabs;

    /// <summary>
    /// The Tab which is currently active
    /// </summary>
    private Tab _currentlyActiveTab;

    // Use this for initialization
    void Start() {
        InitTabs();
    }

    private void InitTabs() {

        foreach (Tab tab in GetComponentsInChildren<Tab>()) {
            tab.gameObject.SetActive(false);
        }

        Tab[] tabs = SharedControllerType.IsAdmin ? _enabledAdminTabs : _enabledPlayerTabs;

        for( int i = 0; i < tabs.Length; i++ ) {
            tabs[i].gameObject.SetActive(true);
            tabs[i].SetActiveState(false);
            tabs[i].SetButtonXPosition(i > 0 ? tabs[i - 1].GetRightCornerPos() : 0);
        }

        // Set the first Tab of the Array as the active one
        SetActiveTab(tabs[0]);
    }

    public void SetActiveTab(Tab tabToActivate) {
        if( _currentlyActiveTab ) {
            _currentlyActiveTab.SetActiveState(false);
        }
        tabToActivate.SetActiveState(true);
        _currentlyActiveTab = tabToActivate;
    }

}
