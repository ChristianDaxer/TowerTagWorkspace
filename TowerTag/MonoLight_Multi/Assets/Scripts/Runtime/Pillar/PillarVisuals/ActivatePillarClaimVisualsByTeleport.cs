using UnityEngine;

public class ActivatePillarClaimVisualsByTeleport : MonoBehaviour {
    [SerializeField] private TeleportMovement _teleportMovement;

    public TeleportMovement TeleportMovement {
        set {
            UnregisterEventListeners();
            _teleportMovement = value;
            RegisterEventListeners();
        }
    }

    #region Init

    private void OnEnable() {
        RegisterEventListeners();
    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void RegisterEventListeners() {
        if (_teleportMovement != null) {
            _teleportMovement.Teleporting += OnTeleport;
        }
    }

    private void UnregisterEventListeners() {
        if (_teleportMovement != null) {
            _teleportMovement.Teleporting -= OnTeleport;
        }
    }

    #endregion

    private static void OnTeleport(int oldPillarID, int newPillarID) {
        // don't mess with Lane functionality to activate/deactivate claim visuals
        if (TTSceneManager.Instance.IsInHubScene || TTSceneManager.Instance.IsInCommendationsScene)
            return;

        Pillar[] pillars;
        if (PillarManager.Instance.IsPillarIDValidInCurrentScene(oldPillarID)) {
            pillars = PillarManager.Instance.GetNeighboursByPillarID(oldPillarID);
            SetPillarClaimVisualsActive(pillars, false);
        }

        if (PillarManager.Instance.IsPillarIDValidInCurrentScene(newPillarID)) {
            pillars = PillarManager.Instance.GetNeighboursByPillarID(newPillarID);
            SetPillarClaimVisualsActive(pillars, true);
        }
    }

    private static void SetPillarClaimVisualsActive(Pillar[] pillars, bool setActive) {
        if (pillars == null) {
            if (setActive) {
                Debug.LogError("ActivatePillarClaimVisualsByTeleport.SetPillarClaimVisualsActive: " +
                               "No Pillars to activate, this should not happen!");
            }

            return;
        }

        foreach (Pillar p in pillars) {
            PillarVisualsExtended pillarVisuals = p.PillarVisuals;
            if (pillarVisuals != null) {
                pillarVisuals.SetActivatedByNeighbours(setActive);
            }
            else {
                Debug.LogError("ActivatePillarClaimVisualsByTeleport.SetPillarClaimVisualsActive: " +
                               "No PillarVisualsExtended found on Pillar(" + p.ID + "), this should not happen!");
            }
        }
    }
}