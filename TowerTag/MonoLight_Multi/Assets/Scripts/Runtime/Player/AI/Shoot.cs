using BehaviorDesigner.Runtime.Tasks;
using System;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace AI {
    [TaskCategory("TT Bot")]
    [Serializable]
    public class Shoot : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        private IPlayer _enemy;
        private AIInputController InputController => _botBrain.Value.InputController;
        private Pillar _enemyPillar;
        private Transform Gun => _botBrain.Value.BotWeapon;
        private float _recoil;

        public override void OnStart() {
            _enemy = _botBrain.Value.EnemyPlayer;
            if (_enemy != null)
                _enemyPillar = _enemy.CurrentPillar;
            _recoil = _botBrain.Value.AIParameters.Recoil;
        }

        public override TaskStatus OnUpdate() {
            //fail task if no input controller OR enemy is dead OR enemy teleported to another pillar since the beginning of this task
            if (InputController == null || _enemy == null || _enemy.PlayerHealth == null
                || !_enemy.PlayerHealth.IsAlive || _enemyPillar != _enemy.CurrentPillar)
                return TaskStatus.Failure;
            InputController.Press(GunController.GunControllerState.TriggerAction.Shoot);
            InputController.Release();
            Gun.Rotate(Vector3.up, Random.Range(-_recoil, _recoil), Space.World);
            Gun.Rotate(Vector3.right, Random.Range(-_recoil, _recoil), Space.Self);
            return TaskStatus.Success;
        }
    }
}