using UnityEngine;
//using Valve.VR;

//[RequireComponent(typeof(SteamVR_PlayArea))]
public class SetupBadaboomHyperactionInputModule : MonoBehaviour {

    private PlayerRigBase playerRigBase;

    private void Start() {
        var inputModule = FindObjectOfType<BadaboomHyperactionInputModule>();
        if (inputModule != null)
        {
            if (playerRigBase == null)
            {
                if (!PlayerRigBase.GetInstance(out playerRigBase))
                    return;
            }
            inputModule.SetupHmd(playerRigBase);
        }
    }
}