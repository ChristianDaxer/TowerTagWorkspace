using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using E = System.Linq.Enumerable;
using MF = UnityEngine.Mathf;
using M = System.Math;
using System.Linq;

namespace Rope {

    public static class BezierUtil {

        public static Vector3[] Derive(Vector3[] bez) {
            Debug.Assert(bez.Length >= 2);
            Vector3[] r = new Vector3[bez.Length - 1];
            int n = r.Length;

            for (int dim = 0; dim < 3; dim++) {
                for (int v = 0; v < n; v++) {
                    r[v][dim] = bez[v + 1][dim] - bez[v][dim];
                }
            }
            return r;
        }

        public static IEnumerable<Vector3> AsPointSequence(Vector3[] bez, int sz) {
            float d = 1.0f / (sz - 1);
            return E.Range(0, sz).Select(i => BezEval_Textbook(bez, i * d));
        }

        public static Vector3 BezEval_Textbook(Vector3[] coeffs, float t) {
            var l = coeffs.Length;
            Debug.Assert(l >= 2);
            if (l == 2)
                return Vector3.Lerp(coeffs[0], coeffs[1], t);
            var tmp = new Vector3[l - 1];
            for (int i = 0; i < l - 1; i++) {
                tmp[i] = Vector3.Lerp(coeffs[i], coeffs[i + 1], t);
            }
            return BezEval_Textbook(tmp, t);
        }

    }

    public static class HermiteUtil {
        public class RecurCoeff {
            public RecurCoeff(int k) {
                Fy0 = 0;
                Fx0 = 0;
                FPi = new float[k + 2];
                // curve f_0 has hard points P_0 , P_1
                // curves f_0,f_n have P_0,P_1,...,P_(n+1)
                // P0 and P_(n+1) are never needed and always null, but this way the indices match up nicely with the indices for control points
                // - less of a headache like this
            }
            public float Fy0 { get; set; }
            public float Fx0 { get; set; }
            public float[] FPi { get; }
            public override string ToString() {
                return $"x0: [{Fx0}] y0: [{Fy0}] Pi:  " + string.Join(" ", FPi.Select(x => x.ToString(CultureInfo.CurrentCulture)).ToArray());
            }
        }

        private static RecurCoeff YN_TO_Y0(int k) {
            if (Cache.ContainsKey(k))
                return Cache[k];

            Debug.Assert(k >= 0);
            var r = new RecurCoeff(k);
            if (k == 0) {
                r.Fy0 = 1;
                r.Fx0 = 0;
                return r;
            }
            else if (k == 1) {
                r.Fy0 = -4;
                r.Fx0 = 1;
                r.FPi[1] = 4;
                return r;
            }

            var rKm1 = YN_TO_Y0(k - 1);
            var rKm2 = YN_TO_Y0(k - 2);
            r.FPi[k] = 4;
            r.FPi[k - 1] = 2;
            r.Fx0 = -4 * rKm1.Fx0 - rKm2.Fx0;
            r.Fy0 = -4 * rKm1.Fy0 - rKm2.Fy0;
            for (int i = 0; i < rKm1.FPi.Length; i++) {
                r.FPi[i] += -4 * rKm1.FPi[i];
            }

            for (int i = 0; i < rKm2.FPi.Length; i++) {
                r.FPi[i] += -rKm2.FPi[i];
            }

            Cache[k] = r;
            return r;
        }

        private static readonly Dictionary<int, RecurCoeff> Cache = new Dictionary<int, RecurCoeff>();

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        private static void PropagateL2R(
           ref Vector3 x0, ref Vector3 y0, ref Vector3 p1,
           ref Vector3 x1, ref Vector3 y1) {
            y1 = 4 * p1 - 4 * y0 + x0;
            x1 = 2 * p1 - y0;
        }

        public static void SingleHermite(
            Vector3[] ps,
            ref Vector3[] xYs,
            Vector3 x0,
            Vector3 yk,
            int noPatches) {
            Debug.Assert(noPatches >= 1);
            int z = noPatches - 1;                   // index to the recur_coeffs
            Debug.Assert(ps.Length >= (noPatches + 2));

            RecurCoeff coeff = YN_TO_Y0(z);
            Debug.Assert(coeff.FPi.Length == (noPatches + 1));

            Vector3 y0 = yk;
            for (int i = 0; i < coeff.FPi.Length; i++) {
                y0 -= ps[i] * coeff.FPi[i];
            }

            y0 -= coeff.Fx0 * x0;
            y0 /= coeff.Fy0;

            xYs[0] = x0;
            xYs[1] = y0;

            for (int i = 0; i < noPatches; i++) {
                int iCurr = 2 * i;
                int iNext = 2 * (i + 1);
                PropagateL2R(ref xYs[iCurr], ref xYs[iCurr + 1], ref ps[i + 1], ref xYs[iNext], ref xYs[iNext + 1]);
            }

        }

    }

    public class SemiTransform {
        // the main purpose is to be able to have a local copy of a Transform and thus gain precise control over when positions are updated
        public Vector3 Position { get; private set; }
        private Quaternion Rotation { get; set; }
        public void SetFrom(Transform t) {
            Position = t.position;
            Rotation = t.rotation;
        }
        // copied from Decompiled UnityEngine.Transform to behave exactly like the original
        public Vector3 Forward => Rotation * Vector3.forward;
    }


    public class RopePhysicsInstance {
        public int CurrentN { get; private set; }

        public Vector3[] Ps { get; private set; }
        public Vector3[] Forces { get; private set; }
        public Vector3[] XYs { get; private set; }
        public Quaternion[] Rots { get; private set; }
        public SemiTransform GunTr { get; } = new SemiTransform();
        public SemiTransform HookTr { get; } = new SemiTransform();

        private Vector3[] _axes;
        private int _maxN;      // both inclusive anchor and specially treated dynamic points at the opposite end

        public void HardReset(RopePhysicsConfig conf, Transform gunTr, Transform hookTr) {
            _maxN = conf.MaxN;

            Debug.Assert(_maxN >= 2);

            if (Ps == null || Ps.Length < _maxN)
                Ps = new Vector3[_maxN];
            if (Forces == null || Forces.Length < _maxN)
                Forces = new Vector3[_maxN];
            if (_axes == null || _axes.Length < _maxN)
                _axes = new Vector3[_maxN];
            if (XYs == null || XYs.Length < 2 * _maxN)
                XYs = new Vector3[2 * _maxN];
            if (Rots == null || Rots.Length < _maxN)
                Rots = new Quaternion[_maxN];

            foreach (int i in E.Range(0, _maxN)) {
                Ps[i] = new Vector3();
                Forces[i] = new Vector3();
                var hookTrPosition = hookTr.position;
                var gunTrPosition = gunTr.position;
                _axes[i] = (hookTrPosition - gunTrPosition).normalized; // <- todo: remove - not needed anymore
                Rots[i] = Quaternion.FromToRotation(Vector3.forward, hookTrPosition - gunTrPosition);
            }

            CurrentN = 2;

            UpdateTransforms(gunTr, hookTr, conf);
        }

        public void UpdateTransforms(Transform gunTr, Transform hookTr, RopePhysicsConfig conf) {
            GunTr.SetFrom(gunTr);
            HookTr.SetFrom(hookTr);
            Ps[TailI] = GunTr.Position;
            Ps[0] = HookTr.Position;  // this kinda sucks <- due to hookTR being constantly overwritten ( because it's a child of the gun and moves along with it )
                                                // orientations
            Quaternion gunOrientation = Quaternion.FromToRotation(Vector3.forward, -gunTr.forward);
            Quaternion nextToGunOrientation = Quaternion.FromToRotation(Vector3.forward, Ps[TailI] - Ps[TailI - 1]);
            Rots[TailI] = Quaternion.Slerp(gunOrientation, nextToGunOrientation, conf.TurnHackFactor);
            Rots[0] = Quaternion.FromToRotation(Vector3.forward, gunTr.position - hookTr.position);
        }

        public bool PushCP_anchor_segment ( ref Vector3 nuP)
        {
            if (CurrentN >= (_maxN - 1)) return false;

            for ( int i = CurrentN; i > 1; i -- )
            {
                Ps[i] = Ps[i - 1];
                Forces[i] = Forces[i - 1];
                Rots[i] = Rots[i - 1];
            }
            CurrentN += 1;
            Ps[1] = nuP;

            return true;
        }

        //                                                        | gun|
        //    anchor |Q------o-------o +++ o----o--------o--------|-----(o)---------------x---x---x---x
        //            0      1       2                             currentN-1                      maxN-1
        // Quaternions in Rots point gun wards and relative to global-z, such that the resting point of PS[i]
        // with respect to its predecessor is:
        // Ps[i-1] + rest_length * (   Rots[i-1]    * (0,0,1 ) )
        // and success respectively:
        // Ps[i+1] + rest_length * ( (-Rots[i+1]) * (0,0,1) )

        public int TailI { get { return CurrentN - 1; } }
        public IEnumerable<int> AllBetweenI { get { return E.Range(1, M.Max(0, CurrentN - 2)); } }
        public IEnumerable<int> AllButAnchorI { get { return E.Range(1, M.Max(0, CurrentN - 1)); } }
        }



    public static class RPNumerics {
        /// ---------------------------------------------

        public static void AxForceAt_wRot(
            ref Vector3 refP,                    // force vectorField origin
            Quaternion rot,                     // force Axis Orientation           - given as relative to global-Z
            ref Vector3 tarP,                    // the point the Force acts upon
            out Vector3 f,                       // resulting Force on tarP
            RopePhysicsConfig conf,
            bool flipQ = false,
            float firstElemHack = 1.0f
        ) {

            float restLen = firstElemHack * conf.RestLength;

            Vector3 v = rot * Vector3.forward;
            if (flipQ)
                v = -v;

            // ------- copy and paste ----------
            Vector3 x = tarP - refP;
            float xMag = x.magnitude;
            Vector3 target = refP + v * restLen;
            Vector3 dTarget = target - tarP;
            float dTargetMagSq = Vector3.Dot(dTarget, dTarget);
            if (dTargetMagSq < conf.Eps) { f = new Vector3(); return; }

            float vdx = Vector3.Dot(v, x);
            float angularFac = 0;
            if (xMag > conf.Eps) {
                float acosArg = M.Max(-1, M.Min(1, (vdx / xMag))); // rounding errors can get this outside of [-1,1] -> Nan
                angularFac = MF.Acos(acosArg);
            }

            float fMag =
                conf.AngularForceScale * angularFac
                + conf.InnerForceScale * MF.Abs(restLen - xMag);

            f = dTarget * ((1 / MF.Sqrt(dTargetMagSq)) * fMag);
            f += conf.Gravity;
            // -----------------------------------
        }
        public static void TargetOrientation(
            ref Vector3 gunward,
            ref Vector3 curr,
            ref Vector3 hookward,
            out Quaternion res
        ) {
            // again, Quaternions rotate (0,0,1) to the gun wards neighbor
            Vector3 curr2Gun = gunward - curr;
            Vector3 hook2Curr = curr - hookward;
            var l1 = curr2Gun.magnitude;
            var l2 = hook2Curr.magnitude;
            Quaternion q1 = Quaternion.FromToRotation(Vector3.forward, curr2Gun);
            Quaternion q2 = Quaternion.FromToRotation(Vector3.forward, hook2Curr);

            float fac = l1 / (l1 + l2);
            fac = float.IsNaN(fac) ? 0 : fac;
            res = Quaternion.Slerp(q1, q2, fac);
        }



        public static void AxForceUpdate_wRot(RopePhysicsInstance rpi, RopePhysicsConfig conf) {
            foreach (var i in rpi.AllBetweenI) {
                rpi.Forces[i] *= conf.InertaDampFactor;

                AxForceAt_wRot(
                    ref rpi.Ps[i - 1],
                    rpi.Rots[i - 1],
                    ref rpi.Ps[i],        // force on Ps[i] from pred
                    out Vector3 resF,
                    conf);

                rpi.Forces[i] += resF;

                AxForceAt_wRot(
                    ref rpi.Ps[i + 1],
                    rpi.Rots[i + 1],
                    ref rpi.Ps[i],        // force on Ps[i] from succ
                    out resF,
                    conf,
                    true,
                    i + 1 == rpi.CurrentN - 1 ? conf.FirstElementHack : 1
                );
                rpi.Forces[i] += resF;
            }
        }
    }
}