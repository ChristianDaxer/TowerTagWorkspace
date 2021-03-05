using System;
using System.Globalization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attention !!! -> the UI Functions allocate Memory per Frame (OnGUI: 4 kB, UpdateUI(Simple): 1.7 kB or UpdateUI(Advanced): 0.8 kB) so the GC will collect in some time
/// Attention !!! -> the FPS Counter should not run to long time spans, so reset them in intervals, because the Average functions for FPS/FrameTime have some shortcomings:
///                     - the Analytic variants can overflow because the values are added up over the whole timespan
///                     - the Iterative variants can accumulate calculation errors (so the deviation grows over time)
/// </summary>
public class FPSCounter : MonoBehaviour {
    /// <summary>
    /// InputAxis to activate/deactivate FPS Counter.
    /// </summary>
    [Tooltip("InputAxis (set in InputManager) to Listen to activate/deactivate the FPSCounter.")]
    [SerializeField]
    private string _switchActiveInputButtonName = "FPSCounter";

    /// <summary>
    /// Is FPSCounter active at the moment?
    /// </summary>
    [SerializeField] private bool _isActive;

    /// <summary>
    /// FPS of the current frame (inverse of current frame time).
    /// </summary>
    private static int CurrentFPS => Time.deltaTime > 0 ? (int) (1 / Time.deltaTime) : 0;

    /// <summary>
    /// Minimal measured FPS (smallest FPS).
    /// </summary>
    private int _minFPS = int.MaxValue;

    /// <summary>
    /// Maximal measured FPS (best FPS).
    /// </summary>
    private int _maxFPS = int.MinValue;

    /// <summary>
    /// Averaged FPS (averaged with iterative function -> error is growing over time because of accumulating errors).
    /// </summary>
    private float _averageFPSIterative;

    /// <summary>
    /// Averaged FPS (averaged with analytic function sumOfFPS/sampleCount)
    /// </summary>
    private float _averageFPSAnalytic;

    private float AverageFPSAnalytic => _sampleCount > 0 ? _averageFPSAnalytic / _sampleCount : float.MaxValue;
    private int _sampleCount;

    /// <summary>
    /// Minimal measured delta Time (shortest frame) in ms.
    /// </summary>
    private float _minDeltaTime = float.MaxValue;

    /// <summary>
    /// Maximal measured delta Time (longest frame) in ms.
    /// </summary>
    private float _maxDeltaTime = float.MinValue;

    /// <summary>
    /// Averaged measured delta Time (average frame time with iterative function -> error is growing over time because of accumulating errors) in ms.
    /// </summary>
    private float _averageDeltaTimeIterative;

    /// <summary>
    /// Averaged frame time (averaged with analytic function sumOfFrameTime/sampleCount) in ms.
    /// </summary>
    private float _averageDeltaTimeAnalytic;

    private float AverageDeltaTimeAnalytic =>
        _sampleCount > 0 ? _averageDeltaTimeAnalytic / _sampleCount : float.MaxValue;

    /// <summary>
    /// TargetFramerate (framerate we want): to count frames which are below this framerate.
    /// </summary>
    [Tooltip("TargetFramerate (framerate we want): to count frames which are below this framerate.")]
    [SerializeField]
    private float _targetFrameRate = 90f;

    /// <summary>
    /// Number of frames which had lower FPS than targetFPS
    /// </summary>
    private int _numberOfFramesUnderTargetFramerate;

    /// <summary>
    /// Minimal Framerate (minimal acceptable framerate): to count frames which are below this framerate.
    /// </summary>
    [Tooltip("Minimal Framerate (minimal acceptable framerate): to count frames which are below this framerate.")]
    [SerializeField]
    private float _minFramerate = 80f;

    /// <summary>
    /// Number of frames which had lower FPS than minFramerate
    /// </summary>
    private int _numberOfFramesUnderMinFramerate;

    [SerializeField, Tooltip("Period to average the measured values (smooth)")]
    private float _measurePeriod = .5f;

    private int _measurePeriodFrameCounter;
    private float _nextMeasureStart;
    private int _smoothedFPSValue;

    /// <summary>
    /// Init Singleton instance, start FPS counter or deactivate if in release build.
    /// </summary>
    private void Start() {
        StartNextMeasurePeriodForSmoothedFPS();

        if (!AreAllTextFieldsAvailable()) {
            Debug.LogError("FPSCounter.Start: Some UI TextFields not available, deactivate FPSCounter ");
            SetActive(false);
        }

        if (!Debug.isDebugBuild || !AreAllTextFieldsAvailable())
            SetActive(false);
    }

    /// <summary>
    /// Measure FrameTimings of current frame and calculate FPS (and averaged timing) values.
    /// </summary>
    private void Update() {
        if (Input.GetButtonDown(_switchActiveInputButtonName))
            SetActive(!_isActive);

        if (!_isActive)
            return;

        // frame based FPS
        // calculate average FPS with analytic function (sum up all fps and divide by counted frames, Attention this can lead to an Overflow so we have to catch this)
        int current = CurrentFPS;
        try {
            _averageFPSAnalytic += current;
        }
        catch (OverflowException e) {
            Debug.LogError(
                "FPSCounter.Update: OverflowException for AverageFPS_Analytic -> ResetFrameCounter is called!");
            Debug.LogError(e.Message);
            ResetFrameCounter();
        }

        // calculate average FPS with iterative function (Attention this can lead to error accumulation)
        _averageFPSIterative = HelperFunctions.CalculateIncrementalAverage(_averageFPSIterative, _sampleCount, current);

        // check if this frames FPS is new min or max FPS
        if (_minFPS > current)
            _minFPS = current;

        if (_maxFPS < current)
            _maxFPS = current;

        // check if this frame is below targetFPS or minFPS
        if (_targetFrameRate > current)
            _numberOfFramesUnderTargetFramerate++;

        if (_minFramerate > current)
            _numberOfFramesUnderMinFramerate++;

        // frame timings
        // calculate average frame time with analytic function (sum up all frame times and divide by counted frames, Attention this can lead to an Overflow so we have to catch this)
        float deltaTime = Time.deltaTime * 1000;
        try {
            _averageDeltaTimeAnalytic += deltaTime;
        }
        catch (OverflowException e) {
            Debug.LogError(
                "FPSCounter.Update: OverflowException for AverageDeltaTime_Analytic -> ResetFrameCounter is called!");
            Debug.LogError(e.Message);
            ResetFrameCounter();
        }

        // calculate average frame time with iterative function (Attention this can lead to error accumulation)
        _averageDeltaTimeIterative =
            HelperFunctions.CalculateIncrementalAverage(_averageDeltaTimeIterative, _sampleCount, deltaTime);


        // check if this frames delta time is new min or max frame time
        if (_minDeltaTime > deltaTime)
            _minDeltaTime = deltaTime;

        if (_maxDeltaTime < deltaTime)
            _maxDeltaTime = deltaTime;

        // update sampleCount (do this after the calculations of the iterative functions and before the call to the analytic functions)
        _sampleCount++;

        // smoothed FPS to better visualize in UI (averaged over _measurePeriod (default is 0.5 s))
        _measurePeriodFrameCounter++;
        if (Time.realtimeSinceStartup > _nextMeasureStart)
            StartNextMeasurePeriodForSmoothedFPS();

        // Update UI Elements with calculated values
        UpdateUI();
    }

    #region UI-Varianten

    #region Advanced Canvas UI (less allocations than ONGUI and Simple Canvas UI, but annoying canvas setup, use this with FPSCounterPrefab)

    /// <summary>
    /// Show current measured frameTimings and FPS values on Screen.
    /// Advanced UI-Text solution (avoid appending strings only allocates around 0.8 kB per Frame (instead of 1.7 kB per Frame in Simple UI Solution or 4kB in OnGUI))
    /// </summary>
    [SerializeField] private GameObject _uiParent;
    [SerializeField] private Text _currentFPSText;
    [SerializeField] private Text _smoothedFPSText;
    [SerializeField] private Text _minFPSText;
    [SerializeField] private Text _maxFPSText;
    [SerializeField] private Text _avgFPSText;
    [SerializeField] private Text _currentFrameTimeText;
    [SerializeField] private Text _minFrameTimeText;
    [SerializeField] private Text _maxFrameTimeText;
    [SerializeField] private Text _avgFrameTimeText;
    [SerializeField] private Text _targetFPSText;
    [SerializeField] private Text _numFramesBelowTargetFPSText;
    [SerializeField] private Text _minFrameRateText;
    [SerializeField] private Text _numFramesBelowMinFPSText;
    [SerializeField] private Text _rTT;
    [SerializeField] private Text _vSync;

    private void UpdateUI() {
        if (!_isActive)
            return;

        _currentFPSText.text = CurrentFPS.ToString();
        _smoothedFPSText.text = _smoothedFPSValue.ToString();
        _minFPSText.text = _minFPS.ToString();
        _maxFPSText.text = _maxFPS.ToString();
        _avgFPSText.text = AverageFPSAnalytic.ToString(CultureInfo.CurrentCulture);

        _currentFrameTimeText.text = (Time.deltaTime * 1000).ToString("F2");
        _minFrameTimeText.text = _minDeltaTime.ToString("F2");
        _maxFrameTimeText.text = _maxDeltaTime.ToString("F2");
        _avgFrameTimeText.text = AverageDeltaTimeAnalytic.ToString(CultureInfo.CurrentCulture);

        _targetFPSText.text = _targetFrameRate.ToString(CultureInfo.CurrentCulture);
        _numFramesBelowTargetFPSText.text = _numberOfFramesUnderTargetFramerate.ToString();
        _minFrameRateText.text = _minFramerate.ToString(CultureInfo.CurrentCulture);
        _numFramesBelowMinFPSText.text = _numberOfFramesUnderMinFramerate.ToString();
        _rTT.text = PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime.ToString();
        _vSync.text = QualitySettings.vSyncCount == 0 ? "disabled" : "enabled";
    }

    private bool AreAllTextFieldsAvailable() {
        return _currentFPSText != null && _smoothedFPSText != null && _minFPSText != null && _maxFPSText != null &&
               _avgFPSText != null
               && _currentFrameTimeText != null && _minFrameTimeText != null && _maxFrameTimeText != null &&
               _avgFrameTimeText != null
               && _targetFPSText != null && _numFramesBelowTargetFPSText != null && _minFrameRateText != null &&
               _numFramesBelowMinFPSText != null;
    }

    #endregion

    #endregion

    /// <summary>
    /// Activate or deactivate the FPS Counter (calculations & display).
    /// </summary>
    /// <param name="setActive">true to activate the counter, false to deactivate it</param>
    private void SetActive(bool setActive) {
        if (setActive != _isActive) {
            _isActive = setActive;
            ResetFrameCounter();

            if (_uiParent != null)
                _uiParent.SetActive(_isActive);
        }
    }

    /// <summary>
    /// Reset FrameCounter member to default values.
    /// </summary>
    private void ResetFrameCounter() {
        StartNextMeasurePeriodForSmoothedFPS();

        // frame based FPS
        _minFPS = int.MaxValue;
        _maxFPS = int.MinValue;
        _averageFPSIterative = 0;
        _averageFPSAnalytic = 0;
        _sampleCount = 0;

        // frame timings
        _minDeltaTime = float.MaxValue;
        _maxDeltaTime = float.MinValue;
        _averageDeltaTimeIterative = .0f;
        _averageDeltaTimeAnalytic = 0;

        // targetFramerate
        _numberOfFramesUnderTargetFramerate = 0;
        _numberOfFramesUnderMinFramerate = 0;
    }

    /// <summary>
    /// Calculate average values of last measure period and set startTime for next period.
    /// </summary>
    private void StartNextMeasurePeriodForSmoothedFPS() {
        _smoothedFPSValue = (int) (_measurePeriodFrameCounter / _measurePeriod);
        _measurePeriodFrameCounter = 0;
        _nextMeasureStart = Time.realtimeSinceStartup + _measurePeriod;
    }
}