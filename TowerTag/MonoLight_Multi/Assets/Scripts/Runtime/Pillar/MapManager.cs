using TowerTag;
using UnityEngine;

public class MapManager : MonoBehaviour {
    private void Start() {
        Pillar[] goalTower = PillarManager.Instance.GetAllGoalPillarsInScene();
        Pillar[] teamBasedTower = PillarManager.Instance.GetAllTeamBasedPillarsInScene();
        switch (GameManager.Instance.CurrentMatch.GameMode) {
            case GameMode.Elimination:
                goalTower.ForEach(gT => {
                    gT.IsGoalPillar = false;
                    SetPillarToOriginalNeutral(gT);
                });
                teamBasedTower.ForEach(tBT => {
                    tBT.IsTeamBased = false;
                    SetPillarToOriginalNeutral(tBT);
                });
                break;
            case GameMode.DeathMatch:
                goalTower.ForEach(gT => {
                    gT.IsGoalPillar = false;
                    if(!gT.IsTeamBased)
                        SetPillarToOriginalNeutral(gT);
                });
                break;
            case GameMode.GoalTower:
                teamBasedTower.ForEach(tBT => {
                    tBT.IsTeamBased = false;
                    if(!tBT.IsGoalPillar)
                        SetPillarToOriginalNeutral(tBT);
                });
                break;
        }
    }

    private void SetPillarToOriginalNeutral(Pillar pillar) {
        pillar.OriginalOwnerTeamID = TeamID.Neutral;
        pillar.ResetOwningTeam();
    }
}