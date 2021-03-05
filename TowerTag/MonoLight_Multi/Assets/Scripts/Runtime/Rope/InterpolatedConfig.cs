using UnityEngine;
using UnityEngine.Serialization;


namespace Rope {
    public class InterpolatedConfig : RopePhysicsConfig {
        [FormerlySerializedAs("_T")]
        [SerializeField]
        [Range(0, 1)]
        float _t;

        public float T { get => _t;
            set => UpdateIntpFactor(value);
        }

        [FormerlySerializedAs("baseConf"), SerializeField] private RopePhysicsConfig _baseConf;   // T = 0
        [FormerlySerializedAs("otherConf"), SerializeField] private RopePhysicsConfig _otherConf;  // T = 1
        public void UpdateIntpFactor(float t) {
            _t = Mathf.Clamp01(t);
            RestLength = Mathf.Lerp(_baseConf.RestLength, _otherConf.RestLength, _t);
            InnerForceScale = Mathf.Lerp(_baseConf.InnerForceScale, _otherConf.InnerForceScale, _t);
            AngularForceScale = Mathf.Lerp(_baseConf.AngularForceScale, _otherConf.AngularForceScale, _t);
            InnerForceScale = Mathf.Lerp(_baseConf.InnerForceScale, _otherConf.InnerForceScale, _t);
            Gravity = Vector3.Lerp(_baseConf.Gravity, _otherConf.Gravity, _t);
            InertaDampFactor = Mathf.Lerp(_baseConf.InertaDampFactor, _otherConf.InertaDampFactor, _t);
            FirstElementHack = Mathf.Lerp(_baseConf.FirstElementHack, _otherConf.FirstElementHack, _t);
            GunCpLengthFactor = Mathf.Lerp(_baseConf.GunCpLengthFactor, _otherConf.GunCpLengthFactor, _t);
            HookCpLengthFactor = Mathf.Lerp(_baseConf.HookCpLengthFactor, _otherConf.HookCpLengthFactor, _t);
            TurnHackFactor = Mathf.Lerp(_baseConf.TurnHackFactor, _otherConf.TurnHackFactor, _t);


        }


        public override void AdaptRestLength(float targetDistance, RopePhysicsInstance rpi) {
            _baseConf.AdaptRestLength(targetDistance, rpi);
            _otherConf.AdaptRestLength(targetDistance, rpi);
            RestLength = Mathf.Lerp(_baseConf.RestLength, _otherConf.RestLength, _t);
        }


    }
}