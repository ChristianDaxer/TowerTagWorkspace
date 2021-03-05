using UnityEngine;

public class HoloPopupAnimationEventHandler : StateMachineBehaviour {

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        SetHoloPopUpAnimationState(true, animator);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        SetHoloPopUpAnimationState(false, animator);
    }

    private void SetHoloPopUpAnimationState(bool status, Animator animator) {
        var holoPopUp = animator.gameObject.GetComponent<HoloPopUp>();
        if (holoPopUp == null) {
            Debug.LogError("Can't find HoloPopUp Behaviour on animators gameObject.");
            return;
        }

        if(!status)
            holoPopUp.ResetHoloPopUpAnimation();
        holoPopUp.AnimatorIsRunning = status;
    }
}
