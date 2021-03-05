using System;
using GameManagement;
using NSubstitute;
using TowerTag;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Action/Test")]
public class TestGameAction : GameAction<TestGameActionParameter> {
    protected override byte EventCode => 99;
    protected override byte DenyEventCode => 98;
    [SerializeField] private bool _alwaysAccept;

    protected override bool IsValid(int senderId, TestGameActionParameter parameters) {
        return _alwaysAccept || parameters.Valid;
    }

    protected override void Execute(int senderId, TestGameActionParameter parameters) {
        Debug.Log($"Executing TestGameAction {parameters}");
    }

    protected override void Deny(int senderId, TestGameActionParameter parameters) {
        Debug.Log($"Denying TestGameAction {parameters}");
    }

    protected override void Rollback(int senderId, TestGameActionParameter parameters) {
        Debug.Log($"Rolling back TestGameAction {parameters}");
    }

    [ContextMenu("Trigger Valid")]
    public void TriggerValid() {
        Trigger(new TestGameActionParameter {Valid = true});
    }

    [ContextMenu("Trigger Invalid")]
    public void TriggerInvalid() {
        Trigger(new TestGameActionParameter {Valid = false});
    }

    [ContextMenu("Init")]
    public void InitTest() {
        Init(Substitute.For<PhotonService>(), Substitute.For<IGameManager>(), Substitute.For<IPlayerManager>(),
            Substitute.For<ISceneService>());
    }
}

[Serializable]
public class TestGameActionParameter : GameActionParameter {
    // ReSharper disable once InconsistentNaming
    public bool Valid;

    public override string ToString() {
        return $"TestGameActionParameter(valid = {Valid})";
    }

    protected override object[] SerializeParameters() {
        return new object[] {Valid};
    }

    protected override void DeserializeParameters(object[] objects) {
        Valid = (bool) objects[0];
    }
}