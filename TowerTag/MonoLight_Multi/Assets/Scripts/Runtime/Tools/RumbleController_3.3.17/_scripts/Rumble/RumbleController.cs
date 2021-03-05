using UnityEngine;
using Bhaptics.Tact.Unity;
using JetBrains.Annotations;

public class RumbleController : MonoBehaviour, IRumbleController {
    [SerializeField] string port;

    [SerializeField] int baudRate;

    // OneShots
    [SerializeField] RumbleCurveSetup _shootProjectile;
    [SerializeField] RumbleCurveSetup _shootProjectileEmpty;
    [SerializeField] RumbleCurveSetup _shootChargerBeam;
    [SerializeField] RumbleCurveSetup _playerHit;

    // looping
    [SerializeField] RumbleCurveSetup _chargerBeamLaser;
    [SerializeField] RumbleCurveSetup _charge;
    [SerializeField] RumbleCurveSetup _playerHeal;
    [SerializeField] RumbleCurveSetup _pillarHighlight;

    [SerializeField] bool _enableRumbleController = true;

    [Space, Header("bHaptics Settings"), SerializeField, Tooltip("Drag the HapticFeedbackDetector of the Gun here")]
    private TactSender gunFeedbackDetector;

    [UsedImplicitly]
    public void EnableControllerRumble(bool enableRumble, bool enabledArduinoGun) {
        ArduinoController.MuteCommunication = !enabledArduinoGun;
        _enableRumbleController = enableRumble;

        if (enableRumble) {
            if (enabledArduinoGun) {
                ArduinoController.Connect(port, baudRate);
                StopAllRumbling();
            }
            else {
                StopAllRumbling();
                ArduinoController.Disconnect();
            }
        }
        else {
            StopAllRumbling();
            ArduinoController.Disconnect();
        }
    }

    private void Start() {
        bool useArduinoRumble = ConfigurationManager.Configuration.EnableRumbleController;
        port = ConfigurationManager.Configuration.ArduinoCOMPortName;
        if (useArduinoRumble && (string.IsNullOrEmpty(port) || !port.Contains("COM"))) {
            Debug.LogWarning("The given COM Port (" + port + ")name from configuration file is not valid!");
            useArduinoRumble = false;
        }

        EnableControllerRumble(_enableRumbleController, useArduinoRumble);
    }

    private void OnApplicationQuit() {
        EnableControllerRumble(false, false);
    }

    public void StopAllRumbling() {
        if (_shootProjectile != null && _shootProjectile.IsPlaying)
            _shootProjectile.Stop();

        if (_shootProjectileEmpty != null && _shootProjectileEmpty.IsPlaying)
            _shootProjectileEmpty.Stop();

        if (_shootChargerBeam != null && _shootChargerBeam.IsPlaying)
            _shootChargerBeam.Stop();

        if (_playerHit != null && _playerHit.IsPlaying)
            _playerHit.Stop();

        if (_chargerBeamLaser != null && _chargerBeamLaser.IsPlaying)
            _chargerBeamLaser.Stop();

        if (_charge != null && _charge.IsPlaying)
            _charge.Stop();

        if (_playerHeal != null && _playerHeal.IsPlaying)
            _playerHeal.Stop();

        if (_pillarHighlight != null && _playerHeal.IsPlaying)
            _pillarHighlight.Stop();
    }

    // OneShots
    // pro Projectile
    public void TriggerShootProjectile() {
        if (!_enableRumbleController)
            return;

        //       Debug.Log("RumbleController.TriggerShot");
        _shootProjectile.TriggerOneShot();

        if (gunFeedbackDetector) {
            gunFeedbackDetector.Play();
        }
    }

    // pro Projectile if energy empty
    public void TriggerShootProjectileEmpty() {
        if (!_enableRumbleController)
            return;

//       Debug.Log("RumbleController.TriggerShot_lowEnergy");
        _shootProjectileEmpty.TriggerOneShot();

        if (gunFeedbackDetector) {
            gunFeedbackDetector.Play();
        }
    }

    // start roll out
    public void TriggerShootChargerBeam() {
        if (!_enableRumbleController)
            return;

//        Debug.Log("RumbleController.TriggerShootChargerBeam");
        _shootChargerBeam.TriggerOneShot();

        if (gunFeedbackDetector) {
            gunFeedbackDetector.Play();
        }
    }

    // local player got hit
    public void TriggerPlayerWasHit() {
        if (!_enableRumbleController)
            return;

//        Debug.Log("RumbleController.TriggerPlayerWasHit");
        _playerHit.TriggerOneShot();
    }

    // toggle on/off (enable: on, !enable: off)
    public void ToggleShootProjectile(bool enable) {
        if (!_enableRumbleController)
            return;

//        Debug.Log("RumbleController.ToggleShootProjectile: " + enable);
        if (enable) {
            _shootProjectile.StartLoop();
        }
        else {
            _shootProjectile.StopLoop();
        }
    }


    // leaving gun until hitting pillar
    public void ToggleChargerBeamLaser(bool enable) {
        if (!_enableRumbleController)
            return;

        //       Debug.Log("RumbleController.ToggleChargerBeamLaser: " + enable);
        if (enable) {
            _chargerBeamLaser.StartLoop();
        }
        else {
            _chargerBeamLaser.StopLoop();
        }
    }

    // rope connected until disconnect
    public void ToggleCharge(bool enable) {
        if (!_enableRumbleController)
            return;

        //       Debug.Log("RumbleController.ToggleCharge: " + enable);
        if (enable) {
            _charge.StartLoop();
        }
        else {
            _charge.StopLoop();
        }
    }

    public void ToggleHealPlayer(bool enable) {
        if (!_enableRumbleController)
            return;

//        Debug.Log("RumbleController.ToggleHealPlayer: " + enable);
        if (enable) {
            _playerHeal.StartLoop();
        }
        else {
            _playerHeal.StopLoop();
        }
    }

    public void ToggleHighlightPillar(bool enable) {
        if (!_enableRumbleController || !ConfigurationManager.Configuration.SingleButtonControl)
            return;

//        Debug.Log("RumbleController.ToggleHighlightPillar: " + enable);
        if (enable) {
            _pillarHighlight.StartLoop();
        }
        else {
            _pillarHighlight.StopLoop();
        }
    }

    private void OnDestroy() {
        StopAllRumbling();

        port = null;
        _shootProjectile = null;
        _shootProjectileEmpty = null;
        _shootChargerBeam = null;
        _playerHit = null;
        _chargerBeamLaser = null;
        _charge = null;
        _playerHeal = null;
        _pillarHighlight = null;
    }
}