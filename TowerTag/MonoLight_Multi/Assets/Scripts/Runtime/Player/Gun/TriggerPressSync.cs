using UnityEngine;


public class TriggerPressSync : MonoBehaviour {

    [SerializeField] private Vector3 _startRotTrigger, _endRotTrigger;
    [SerializeField] private GameObject _ownTriggerObj;

    private void Start() {


        PlayerInput input_right = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput input_left = (PlayerInput)PlayerInputBase.leftHand;

        if (input_right != null)
            input_right.OnTriggerStateValue += OnTriggerThresholdAction;

        if (input_left != null) 
            input_left.OnTriggerStateValue += OnTriggerThresholdAction;
    }

    private void OnDestroy() {

        PlayerInput input_right = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput input_left = (PlayerInput)PlayerInputBase.leftHand;

        if (input_right != null)
            input_right.OnTriggerStateValue -= OnTriggerThresholdAction;

        if (input_left != null)
            input_left.OnTriggerStateValue -= OnTriggerThresholdAction;
    }
    private void OnTriggerThresholdAction(PlayerInputBase fromSource, float newAxis)
    {
        _ownTriggerObj.transform.localEulerAngles = Vector3.Lerp(_startRotTrigger, _endRotTrigger, newAxis);
    }
}