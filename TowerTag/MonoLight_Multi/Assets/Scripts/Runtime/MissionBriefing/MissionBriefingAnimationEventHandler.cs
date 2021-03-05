using JetBrains.Annotations;
using UnityEngine;

public class MissionBriefingAnimationEventHandler : MonoBehaviour
{
    private static readonly int _play = Animator.StringToHash("play");

    public TeamPinManager PinManager { get; set; }
    public MissionBriefingController MissionBriefingController { get; set; }

    [UsedImplicitly]
    private void PlayInfoPin1()
    {
        if (PinManager == null)
            return;

        PinManager.OwnTeamPinAnimators.ForEach(animator => {
            if (animator != null) animator.SetBool(_play, true);
        });
    }

    [UsedImplicitly]
    private void PlayInfoPin2()
    {
        if (PinManager == null)
            return;

        PinManager.EnemyTeamPinAnimators.ForEach(animator => {
            if (animator != null)
                animator.SetBool(_play, true);
        });

    }

    [UsedImplicitly]
    private void StopInfoPin1()
    {
        if (PinManager == null)
            return;

        PinManager.OwnTeamPinAnimators.ForEach(animator => {
            if (animator != null)
                animator.SetBool(_play, false);
        });

    }

    [UsedImplicitly]
    private void StopInfoPin2() {
        if (PinManager == null) return;
        PinManager.EnemyTeamPinAnimators.ForEach(animator => {
            if (animator != null)
                animator.SetBool(_play, false);
        });
    }

    [UsedImplicitly]
    private void FinishMissionBriefing() {
        MissionBriefingController.FinishMissionBriefing();
    }
}
