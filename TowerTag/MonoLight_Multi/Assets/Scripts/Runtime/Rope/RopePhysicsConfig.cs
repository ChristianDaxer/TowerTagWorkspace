using UnityEngine;
using System.Collections.Generic;
using E = System.Linq.Enumerable;
using System.Linq;
using UnityEngine.Serialization;

namespace Rope {
    public class RopePhysicsConfig : MonoBehaviour {

        [Header("automatic at runtime from rest_length_scale")]
        [FormerlySerializedAs("rest_length"), SerializeField]
        private float _restLength = 1.0f;
        public float RestLength { get => _restLength;
            protected set => _restLength = value; }
        [FormerlySerializedAs("rest_length_scale"), SerializeField]
        private float _restLengthScale = 1.0f;

        private float RestLengthScale => _restLengthScale;

        [Space]
        [FormerlySerializedAs("innerForceScale"), SerializeField]
        private float _innerForceScale;
        public float InnerForceScale { get => _innerForceScale; set => _innerForceScale = value; }


        [FormerlySerializedAs("angularForceScale"),SerializeField]
        private float _angularForceScale = 1.0f;
        public float AngularForceScale { get => _angularForceScale;
            protected set => _angularForceScale = value; }

        [FormerlySerializedAs("gravity"), SerializeField]
        private Vector3 _gravity;
        public Vector3 Gravity { get => _gravity;
            protected set => _gravity = value; }

        [FormerlySerializedAs("inerta_damp_factor"), SerializeField]
        private float _internaDumbFactor = 0.8f;
        public float InertaDampFactor { get => _internaDumbFactor;
            protected set => _internaDumbFactor = value; }

        [Space]
        [FormerlySerializedAs("GunCPLengthFactor"),SerializeField]
        private float _gunCpLengthFactor = 0.1f;
        public float GunCpLengthFactor { get => _gunCpLengthFactor;
            protected set => _gunCpLengthFactor = value; }

        [FormerlySerializedAs("HookCPLengthFactor"), SerializeField]
        private float _hookCpLengthFactor = 0.3f;
        public float HookCpLengthFactor { get => _hookCpLengthFactor;
            protected set => _hookCpLengthFactor = value; }

        [Space]
        [FormerlySerializedAs("maxN"), SerializeField]
        private int _maxN = 30;
        public int MaxN => _maxN;


        [FormerlySerializedAs("eps"),SerializeField]
        private float _eps = 1 / 10000f;
        public float Eps => _eps;

        [Space]
        [FormerlySerializedAs("first_element_hack"),SerializeField]
        private float _firstElementHack = 0.3f;
        public float FirstElementHack { get => _firstElementHack;
            protected set => _firstElementHack = value; }


        [FormerlySerializedAs("turn_hack_factor"), SerializeField]
        private float _turnHackFactor;

        public float TurnHackFactor {
            get => _turnHackFactor;
            protected set => _turnHackFactor = value;
        }

        // ================= RPI state manipulation stuff =======================

        public void FrameUpdatePhys(RopePhysicsInstance rpi, float dt) {
            RPNumerics.AxForceUpdate_wRot(rpi, this);
            foreach (var i in rpi.AllButAnchorI) {
                rpi.Ps[i] += rpi.Forces[i] * dt;
            }

            UpdateAxes(rpi);

        }

        private void UpdateAxes(RopePhysicsInstance rpi) { // <- DummyImplementation

            foreach (var i in rpi.AllBetweenI) {
                RPNumerics.TargetOrientation(
                    ref rpi.Ps[i + 1], // gun ward
                    ref rpi.Ps[i], // curr
                    ref rpi.Ps[i - 1], // hook ward
                    out rpi.Rots[i]);

            }
        }

        // to be called at shooting time - adjust rest length to target, and let the rest by done through physics constants interpolation
        // (also needs a UpdateInputFactor to propagate through )
        public virtual void AdaptRestLength(float targetDistance, RopePhysicsInstance rpi) {
            float d = targetDistance / (rpi.CurrentN - 1); // fencepost
            RestLength = d * RestLengthScale;
        }

        public void Splinalize(RopePhysicsInstance rpi) {
            Vector3[] rpiXYs = rpi.XYs;
            HermiteUtil.SingleHermite(
                rpi.Ps, ref rpiXYs,
                rpi.HookTr.Position + rpi.HookTr.Forward * ((rpi.Ps[0] - rpi.Ps[1]).magnitude * HookCpLengthFactor),
                rpi.GunTr.Position + rpi.GunTr.Forward * ((rpi.Ps[rpi.TailI] - rpi.Ps[rpi.TailI - 1]).magnitude * GunCpLengthFactor),
                rpi.CurrentN - 1);
        }

        public IEnumerable<Vector3[]> GetPatches(RopePhysicsInstance rpi) {
            return E.Range(0, rpi.CurrentN > 0 ? rpi.CurrentN - 1 : 0).Select(i => {
                int xyI = i * 2;
                return new[] { rpi.Ps[i], rpi.XYs[xyI], rpi.XYs[xyI + 1], rpi.Ps[i + 1] };
            });
        }
    }


}