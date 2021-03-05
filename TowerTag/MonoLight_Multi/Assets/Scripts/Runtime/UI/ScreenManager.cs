using System.Collections;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the main menu screens
/// </summary>
public class ScreenManager : MonoBehaviour {
    //Screen to open automatically at the start of the Scene
    [SerializeField] private Animator _initiallyOpen;

    //Currently Open Screen
    private Animator _open;

    //Hash of the parameter we use to control the transitions.
    private int _openParameterId;

    //The GameObject Selected before we opened the current Screen.
    //Used when closing a Screen, so we can go back to the button that opened it.
    private GameObject _previouslySelected;

    //Animator State and Transition names we need to check against.
    private const string KOpenTransitionName = "open";
    private const string KClosedStateName = "Closed";

    private void Awake() {
        foreach (Canvas can in GetComponentsInChildren<Canvas>()) {
            can.gameObject.SetActive(false);
        }
    }

    public void OnEnable() {
        //We cache the Hash to the "Open" Parameter, so we can feed to Animator.SetBool.
        _openParameterId = Animator.StringToHash(KOpenTransitionName);

        //If set, open the initial Screen now.
        if (_initiallyOpen == null)
            return;

        OpenPanel(_initiallyOpen);
    }

    //Closes the currently open panel and opens the provided one.
    //It also takes care of handling the navigation, setting the new Selected element.
    public void OpenPanel(Animator anim) {
        // TODO Reminder: only a temp. quick & dirty solution for the Beta Home-Version.
        if (TowerTagSettings.Home && !SharedControllerType.IsAdmin)
            return;

        if (_open == anim)
            return;

        //Activate the new Screen hierarchy so we can animate it.
        anim.gameObject.SetActive(true);
        //Save the currently selected button that was used to open this Screen. (CloseCurrent will modify it)
        GameObject newPreviouslySelected = EventSystem.current.currentSelectedGameObject;
        //Move the Screen to front.
        anim.transform.SetAsLastSibling();

        CloseCurrent();

        _previouslySelected = newPreviouslySelected;

        //Set the new Screen as then open one.
        _open = anim;
        //Start the open animation
        _open.SetBool(_openParameterId, true);

        SetSelected(null);
    }

    //Closes the currently open Screen
    //It also takes care of navigation.
    //Reverting selection to the Selectable used before opening the current screen.
    private void CloseCurrent() {
        if (_open == null)
            return;

        //Start the close animation.
        _open.SetBool(_openParameterId, false);

        //Reverting selection to the Selectable used before opening the current screen.
        SetSelected(_previouslySelected);
        //Start Coroutine to disable the hierarchy when closing animation finishes.
        StartCoroutine(DisablePanelDelayed(_open));
        //No screen open.
        _open = null;
    }

    //Coroutine that will detect when the Closing animation is finished and it will deactivate the
    //hierarchy.
    private IEnumerator DisablePanelDelayed(Animator anim) {
        var closedStateReached = false;
        var wantToClose = true;
        while (!closedStateReached && wantToClose) {
            if (!anim.IsInTransition(0))
                closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(KClosedStateName);

            wantToClose = !anim.GetBool(_openParameterId);

            yield return new WaitForEndOfFrame();
        }

        if (wantToClose)
            anim.gameObject.SetActive(false);
    }

    //Make the provided GameObject selected
    //When using the mouse/touch we actually want to set it as the previously selected and
    //set nothing as selected for now.
    private static void SetSelected(GameObject go) {
        //Select the GameObject.
        EventSystem eventSystem;
        (eventSystem = EventSystem.current).SetSelectedGameObject(go);

        //If we are using the keyboard right now, that's all we need to do.
        var standaloneInputModule = eventSystem.currentInputModule as StandaloneInputModule;
        if (standaloneInputModule != null)
            return;

        //Since we are using a pointer device, we don't want anything selected.
        //But if the user switches to the keyboard, we want to start the navigation from the provided game object.
        //So here we set the current Selected to null, so the provided gameObject becomes the Last Selected in the EventSystem.
        EventSystem.current.SetSelectedGameObject(null);
    }
}