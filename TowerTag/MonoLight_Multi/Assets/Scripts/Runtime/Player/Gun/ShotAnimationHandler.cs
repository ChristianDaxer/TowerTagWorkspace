using System;
using RotaryHeart.Lib.SerializableDictionary;
using TowerTag;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShotAnimationHandler : MonoBehaviour
{
    [Serializable]
    public class ParticleSystemToShaderID : SerializableDictionaryBase<ParticleSystem, string>
    {
    }

    [SerializeField] private ParticleSystemToShaderID _particleSystemToShaderID;
    private Animator _animator;
    private IPlayer _owner;
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _owner = GetComponentInParent<Player>();
    }

    private void OnEnable()
    {
        ShotManager.Singleton.ShotFired += OnShotFired;
        if (_owner != null)
        {
            _owner.PlayerTeamChanged += OnTeamChanged;
            OnTeamChanged(_owner, _owner.TeamID);
        }
    }

    private void OnTeamChanged(IPlayer player, TeamID teamId)
    {
        ColorizeParticles(TeamManager.Singleton.Get(teamId).Colors.Main);
    }

    private void ColorizeParticles(Color colorsEffect)
    {
        _particleSystemToShaderID.Keys.ForEach(ps =>
            ps.GetComponent<Renderer>().material.SetColor(_particleSystemToShaderID[ps], colorsEffect));
    }

    private void OnShotFired(ShotManager shotManager, string id, IPlayer player, Vector3 position, Quaternion rotation)
    {
        if (player.IsMe)
            _animator.SetTrigger(Shoot);
    }

    private void OnDisable()
    {
        ShotManager.Singleton.ShotFired -= OnShotFired;
        if (_owner != null)
        {
            _owner.PlayerTeamChanged -= OnTeamChanged;
        }
    }
}