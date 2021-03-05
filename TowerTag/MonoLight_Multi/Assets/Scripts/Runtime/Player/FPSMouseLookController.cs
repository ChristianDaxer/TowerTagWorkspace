using System;
using JetBrains.Annotations;
using TowerTagSOES;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(FirstPersonController))]
public class FPSMouseLookController : MonoBehaviour {
    private MouseLook _mouseLook;

    private bool _screenMenuActive;
    private bool _operatorMenuActive;

    private void Awake() {
        if (!SharedControllerType.NormalFPS && !TowerTagSettings.IsHomeTypeOculus)
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start() {
        SetMouseLook(GetComponent<FirstPersonController>());
        GameManagerStateMachineTestUI.StateMachineTestUiToggled += OnStateMachineToggled;
    }

    private void OnStateMachineToggled(bool active) {
        ToggleMouseLock(active);
    }

    private void OnDisable() {
        GameManagerStateMachineTestUI.StateMachineTestUiToggled -= OnStateMachineToggled;
    }

    private void SetMouseLook([NotNull] FirstPersonController fpsController) {
        if (fpsController == null) throw new ArgumentNullException(nameof(fpsController));
        _mouseLook = GetComponent<FirstPersonController>().MouseLook;
    }

    private void ToggleMouseLock(bool state) {
        _mouseLook?.SetCursorLock(!state);
    }
}