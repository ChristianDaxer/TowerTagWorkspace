using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    //*The tilt is real*
    //Tilts the head of the bot around a specified angle on z-axis so that its head peaks out from behind the tower. Looks more natural and cute plus bot has more cover.
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TiltHead : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private float _rotationTime = 0.8f;

        private Transform Head => _botBrain.Value.BotHead;
        private float HeadTiltAmount => _botBrain.Value.AIParameters.HeadTiltAmount;
        private Vector3 _currentPillarPos;
        private float _rotationAngle;
        private Quaternion _targetRotation;
        private float _timePassed;


        public override void OnStart() {
            _currentPillarPos = _botBrain.Value.Player.CurrentPillar.transform.position;
            _rotationAngle = GetAngleDirection(Head.forward, Head.position - _currentPillarPos);
            Vector3 headEulerAngles = Head.eulerAngles;
            _targetRotation = Quaternion.Euler(headEulerAngles.x, headEulerAngles.y, _rotationAngle);
            _timePassed = 0;
        }

        public override TaskStatus OnUpdate() {
            _timePassed += Time.deltaTime;
            float step = _timePassed / _rotationTime;

            Head.rotation = Quaternion.Slerp(Head.rotation, _targetRotation, step);

            if (_timePassed > _rotationTime) {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }


        //returns head tilt amount based on whether the bot is standing on the left or right side of the pillar
        private float GetAngleDirection(Vector3 fwdDir, Vector3 targetDir) {
            Vector3 cross = Vector3.Cross(fwdDir, targetDir);
            float direction = Vector3.Dot(cross, Vector3.up); //scalar product

            if (direction > 0f) //pillar on the left side
            {
                //Debug.Log("############ PILLAR IS LEFT");
                return -HeadTiltAmount;
            }
            else if (direction < 0f) //pillar on the right side
            {
                //Debug.Log("############ PILLAR IS RIGHT");
                return HeadTiltAmount;
            }
            else //pillar right in front || behind bot
            {
                return 0; //dont tilt head
            }
        }
    }
}