using UnityEngine;

public class PickRandomAnim : StateMachineBehaviour
{
    public int _numberOfAnims = 3;
    private static readonly int _animNumber = Animator.StringToHash("AnimNumber");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger(_animNumber, Random.Range(1, _numberOfAnims));
    }
}
