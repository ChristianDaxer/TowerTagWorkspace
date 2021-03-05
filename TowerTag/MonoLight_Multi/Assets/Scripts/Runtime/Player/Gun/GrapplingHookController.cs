using System.Collections;
using TowerTag;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GrapplingHookController : MonoBehaviour
{
    [SerializeField] private RopeGameAction _ropeGameAction;

    private const float DelayForGrapplingHook = 0;
    private GunController _gunController;
    private Animator _grapplingHookAnimator;
    private Coroutine _grapplingHookCoroutine;
    private IPlayer _player;
    private static readonly int _extend = Animator.StringToHash("extend");
    private static readonly int _shoot = Animator.StringToHash("shoot");

    public void Init(IPlayer player)
    {
        _player = player;
        _gunController = player.GunController;
        _player.PlayerHealth.PlayerDied += OnPlayerDied;
    }

    private void Awake()
    {
        _grapplingHookAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _ropeGameAction.RopeConnectedToChargeable += ShootHook;
        _ropeGameAction.Disconnecting += RetriggerHook;
        SceneManager.sceneLoaded += ResetHook;

        if (_player != null && _player.PlayerHealth != null)
            _player.PlayerHealth.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        _ropeGameAction.RopeConnectedToChargeable -= ShootHook;
        _ropeGameAction.Disconnecting -= RetriggerHook;
        SceneManager.sceneLoaded -= ResetHook;

        if (_player != null && _player.PlayerHealth != null)
            _player.PlayerHealth.PlayerDied -= OnPlayerDied;
    }

    private void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType)
    {
        SetAnimatorBools(false, true);
    }

    /// <summary>
    /// Extend the Hook after disconnect, player is still aiming at the same target.
    /// </summary>
    private void RetriggerHook(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose)
    {
        if (ConfigurationManager.Configuration.SingleButtonControl && _player == player && _gunController != null &&
            _player.PlayerHealth.IsAlive)
        {
            //Raycast just locally possible! don't trigger this on remotes
            Chargeable raycastTarget = _gunController.DoRaycast();
            if (raycastTarget.CheckForNull()?.GetComponent<Chargeable>() == target)
            {
                TriggerGrapplingAnimation(true);
            }
        }
    }

    /// <summary>
    /// Set GrapplingHook on idle when the scene changed to ensure it has the correct state
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void ResetHook(Scene scene, LoadSceneMode mode)
    {
        if (_grapplingHookAnimator.GetCurrentAnimatorStateInfo(0).IsName("Forward"))
        {
            SetAnimatorBools(false, true);
        }
    }

    /// <summary>
    /// Set hook to idle
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="player"></param>
    /// <param name="target"></param>
    private void ShootHook(RopeGameAction sender, IPlayer player, Chargeable target)
    {
        if (_player == player)
        {
            if (_grapplingHookAnimator.GetCurrentAnimatorStateInfo(0).IsName("Forward"))
                SetAnimatorBools(false, true);
        }
    }

    /// <summary>
    /// Trigger the animation and sync it
    /// </summary>
    /// <param name="direction"></param>
    public void TriggerGrapplingAnimation(bool direction)
    {
        if (!gameObject.activeInHierarchy) return;

        if (_grapplingHookCoroutine != null)
            StopCoroutine(_grapplingHookCoroutine);

        _grapplingHookCoroutine = StartCoroutine(PlayAnimationDelayed(direction));

        if (_player.IsMe)
            _player.PlayerNetworkEventHandler.SendGrapplingHookUpdate(direction);
    }

    /// <summary>
    /// Handling the animation itself
    /// </summary>
    /// <param name="direction">true = forwards, false = backwards</param>
    /// <returns></returns>
    private IEnumerator PlayAnimationDelayed(bool direction)
    {
        if (direction)
        {
            yield return new WaitForSeconds(DelayForGrapplingHook);
            if (!_grapplingHookAnimator.GetCurrentAnimatorStateInfo(0).IsName("Forward"))
                SetAnimatorBools(true, false);
        }
        else
        {
            yield return new WaitForSeconds(DelayForGrapplingHook);
            if (_grapplingHookAnimator.GetCurrentAnimatorStateInfo(0).IsName("Forward"))
            {
                if (!_player.IsMe || _gunController != null
                    && _gunController.StateMachine.CurrentStateIdentifier !=
                    GunController.GunControllerStateMachine.State.Charge)
                    SetAnimatorBools(false, false);
            }
        }

        _grapplingHookCoroutine = null;
    }

    private void SetAnimatorBools(bool extend, bool shoot)
    {
        _grapplingHookAnimator.SetBool(_extend, extend);
        _grapplingHookAnimator.SetBool(_shoot, shoot);
    }
}