using UnityEngine;

public class RumbleTestGUI : MonoBehaviour
{
    [SerializeField]
    RumbleController _controller;

    bool _chargeToggle;
    bool _healToggle;
    bool _highlightToggle;

    void OnGUI()
    {
        if (GUILayout.Button("Trigger ShootProjectile"))
        {
            _controller.TriggerShootProjectile();
        }

        if (GUILayout.Button("Trigger ShotChargerBeam"))
        {
            _controller.TriggerShootChargerBeam();
        }

        if (GUILayout.Button("Trigger PlayerWasHit"))
        {
            _controller.TriggerPlayerWasHit();
        }

        if (GUILayout.Button("Toggle ChargerBeam"))
        {
            _chargeToggle = !_chargeToggle;
            _controller.ToggleCharge(_chargeToggle);
        }

        if (GUILayout.Button("Toggle HealPlayer"))
        {
            _healToggle = !_healToggle;
            _controller.ToggleHealPlayer(_healToggle);
        }

        if (GUILayout.Button("Toggle HighlightPillar"))
        {
            _highlightToggle = !_highlightToggle;
            _controller.ToggleHighlightPillar(_highlightToggle);
        }
    }
}
