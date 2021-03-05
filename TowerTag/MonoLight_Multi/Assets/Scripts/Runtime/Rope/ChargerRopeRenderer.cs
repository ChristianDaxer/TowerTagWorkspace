using Rope;
using System.Collections;
using System.Collections.Generic;
using TowerTag;
using UnityEngine;

public abstract class ChargerRopeRenderer : ChargerBeamRenderer
{
	protected class InterpolatedVelocities
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming")]
		private struct PinT
		{
			public Vector3 point;
			public float time;
		}

		private PinT[] _pointsInTime;

		private int _pointer; // pseudo-invariant: points to the index directly after the last inserted, on a full ring buff this is the oldest value

		private int _currSz;

		public InterpolatedVelocities(int sz)
		{
			if (sz < 3)
				throw new System.Exception("bug");

			_pointsInTime = new PinT[sz];
		}

		public void Reset()
		{
			_pointer = 0;
			_currSz = 0;
		}

		public void Push(Vector3 point, float time)
		{
			_pointsInTime[_pointer] = new PinT { point = point, time = time };

			_pointer++;
			_pointer %= _pointsInTime.Length;
			_currSz++;
			_currSz = System.Math.Min(_currSz, _pointsInTime.Length);
		}

		public Vector3 ImmediateVelo()
		{
			if (_currSz < 2)
				return new Vector3(0, 0, 0);
			else
			{
				int lastI = (_pointer + _pointsInTime.Length - 1) % _pointsInTime.Length;
				int secondToLastI = (_pointer + _pointsInTime.Length - 2) % _pointsInTime.Length;
				var pint1 = _pointsInTime[lastI];
				var pint2 = _pointsInTime[secondToLastI];
				float dt = pint1.time - pint2.time;
				const float timeEps = 0.01f;
				if (dt < timeEps)
					return new Vector3(0, 0, 0);

				return
					(pint2.point - pint1.point) /
					dt; // <- not quite sure what the obvious direction for this velocity should be, atm it doesn't matter, for only the magnitude of this value is actually used
			}
		}

		private IEnumerable<PinT> Seq()
		{
			if (_currSz < _pointsInTime.Length)
			{
				for (int i = 0; i < _currSz; i++)
				{
					yield return _pointsInTime[i];
				}
			}
			else
			{
				for (int i = 0; i < _pointsInTime.Length; i++)
				{
					yield return _pointsInTime[(_pointer + i) % _pointsInTime.Length];
				}
			}
		}

		public IEnumerable<Vector3> RelativeVelos(Vector3 relPos, float relTime)
		{
			System.Func<Vector3, float, Vector3> v = (pBuff, tBuff) => (relPos - pBuff) / (relTime - tBuff);
			const float timeEps = 0.01f;
			foreach (var p in Seq())
			{
				if ((relTime - p.time) > timeEps)
					yield return v(p.point, p.time);
			}
		}
	}

	public abstract float UpperVelo { get; }
	public abstract Transform SpawnBeamAnchor { get; set; }
	public abstract Transform HookAsset { get; }
	public abstract RopePhysicsInstance RPI { get; }
	public abstract InterpolatedConfig Conf { get; set; }

}
