using UnityEngine;

[DefaultExecutionOrder(-100)]

/* 
 * At the beginning of each frame, the Update() function in this class will tick and force the 
 * PlayerManager to class to update it's categorized cache of arrays containing instances of
 * IPlayer. Before PlayerManager, was making heavy use of Linq and this refactor avoids any
 * unnessary garbage allocations and boxing.
*/
public class PlayerManagerUpdateData : MonoBehaviour
{
    private void Update ()
    {
        PlayerManager.Instance.UpdatePlayerReferencesCache();
    }
}
