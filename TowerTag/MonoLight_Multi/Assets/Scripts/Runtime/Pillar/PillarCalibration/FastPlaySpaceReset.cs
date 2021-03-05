using System;
using System.Collections;
using Runtime.Pillar.PillarCalibration;
using UnityEngine;


public class FastPlaySpaceReset : MonoBehaviour
{

    [SerializeField] private Transform _hmd;

    private bool _menuPressed;

    private bool _triggerPressed;

    private Coroutine _resetCoroutine;
    // Start is called before the first frame update
    private void OnEnable()
    {
        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        if (_rightXRController != null) { 
            _rightXRController.OnTriggerUp += OnTriggerStateChanged;
            _rightXRController.OnTriggerDown += OnTriggerStateChanged;
            _rightXRController.OnMenuDown += MenuButtonClicked;
            _rightXRController.OnMenuUp += MenuButtonReleased;
        }

        if (_leftXRController != null) { 
            _leftXRController.OnTriggerUp += OnTriggerStateChanged;
            _leftXRController.OnTriggerDown += OnTriggerStateChanged;
            _leftXRController.OnMenuDown += MenuButtonClicked;
            _leftXRController.OnMenuUp += MenuButtonReleased;
        }
    }

    private void OnDisable()
    {

        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        if (_rightXRController != null) { 
            _rightXRController.OnTriggerUp -= OnTriggerStateChanged;
            _rightXRController.OnTriggerDown -= OnTriggerStateChanged;
            _rightXRController.OnMenuDown -= MenuButtonClicked;
            _rightXRController.OnMenuUp -= MenuButtonReleased;
        }

        if (_leftXRController != null) { 
            _leftXRController.OnTriggerUp -= OnTriggerStateChanged;
            _leftXRController.OnTriggerDown -= OnTriggerStateChanged;
            _leftXRController.OnMenuDown -= MenuButtonClicked;
            _leftXRController.OnMenuUp -= MenuButtonReleased;
        }
    }
    private void MenuButtonReleased(PlayerInputBase controller)
    {
        _menuPressed = false;
    }

    private void MenuButtonClicked(PlayerInputBase controller)
    {
        if (GameManager.Instance.IsStateMachineInMatchState()) return;
            
        _menuPressed = true;

        if (_menuPressed && _triggerPressed)
        {
            if (_resetCoroutine == null)
                _resetCoroutine = StartCoroutine(ResetPlaySpace());
        }
        else
        {
            if (_resetCoroutine != null)
                StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }
    }

    private void OnTriggerStateChanged(PlayerInputBase controller, bool state)
    {
        if (GameManager.Instance.IsStateMachineInMatchState()) return;
        
        _triggerPressed = state;

        if (_menuPressed && _triggerPressed)
        {
            if (_resetCoroutine == null)
                _resetCoroutine = StartCoroutine(ResetPlaySpace());
        }
        else
        {
            if (_resetCoroutine != null)
                StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }
    }

    private IEnumerator ResetPlaySpace()
    {
        yield return new WaitForSeconds(3);
        if (PillarOffsetManager.Instance == null) yield break;
        Vector3 offset = transform.localPosition - _hmd.localPosition;
        offset.y = 0;
        PillarOffsetManager.Instance.ApplyPillarOffset.SetOffsetConfigurationValues(offset, _hmd.transform.localEulerAngles.y);
        PillarOffsetManager.Instance.ApplyPillarOffset.ApplyOffsetFromConfigurationFile();
        _resetCoroutine = null;
    }
}
