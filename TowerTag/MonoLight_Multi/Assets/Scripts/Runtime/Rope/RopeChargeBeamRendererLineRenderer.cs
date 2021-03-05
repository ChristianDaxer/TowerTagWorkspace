using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TowerTag;
using UnityEngine.Serialization;
using E = System.Linq.Enumerable;
using MF = UnityEngine.Mathf;


namespace Rope
{
	public class RopeChargeBeamRendererLineRenderer : ChargerRopeRenderer
	{
		[SerializeField] private AnimationCurve _stiffnessFadeCurve;

		private RopePhysicsInstance _rpi;

		public override RopePhysicsInstance RPI
		{
			get { return _rpi ?? (_rpi = new RopePhysicsInstance()); }
		}

		[SerializeField, FormerlySerializedAs("Conf")] private InterpolatedConfig _config;
		public override InterpolatedConfig Conf
		{
			get => _config;
			set => _config = value;
		}

		[FormerlySerializedAs("SpawnBeamAnchor"), SerializeField] private Transform _spawnBeamAnchor; // Gun Hard point

		// extra empty to measure movement velocity at , gun hard point at the tip of the gun turned out to move really fast when bending the wrist alone
		[FormerlySerializedAs("VelocityRefPoint"), SerializeField] private Transform _velocityRefPoint;

		protected override IEnumerator AttachFailBehaviour(Chargeable pillar)
		{
			Connect(pillar);
			yield return new WaitForSeconds(_shootingDuration);
			Disconnect();
		}

		public override float Tension => _fadedStiffness;

		// temporal state
		private bool _visible;
		private bool _shootingPhase;

		private float _startTime;
		// controller stiffness stuff

		[Header("only for debug")]
		[FormerlySerializedAs("direct_stiffness"), SerializeField]
		private float _directStiffness;
		[FormerlySerializedAs("faded_stiffness"), SerializeField] private float _fadedStiffness;

		[Space]
		[FormerlySerializedAs("stiffness_fade_delay"), SerializeField]
		private float _stiffnessFadeDelay;

		private float _stiffnessDelayStartT;
		private float _stiffnessDelayAnchor;

		[FormerlySerializedAs("shootingDuration"), SerializeField] private float _shootingDuration = 0.2f;

		[Header("Controller velocity envelope in m/s")]
		[FormerlySerializedAs("angular_cone_rad"), SerializeField]
		private float _angularConeRad = Mathf.PI / 4;

		[FormerlySerializedAs("lowerVelo")]
		[Range(0, 1)]
		[SerializeField]
		private float _lowerVelo = 0.1f;
		[FormerlySerializedAs("upperVelo")] [Range(0, 20)] [SerializeField] private float _upperVelo = 3f;

		public override float UpperVelo => _upperVelo;

		[Space]
		[FormerlySerializedAs("spawn_hack_l0"), SerializeField] private float _spawnHackL0 = 2.0f;
		[FormerlySerializedAs("spawn_hack_l1"), SerializeField] private float _spawnHackL1 = 9.0f;
		[FormerlySerializedAs("spawn_hack_l0_noCPs"), SerializeField] private int _spawnHackL0NoCPs = 4;
		[FormerlySerializedAs("spawn_hack_l1_noCPs"), SerializeField] private int _spawnHackL1NoCPs = 7;
		[FormerlySerializedAs("shooting_phase_distort"), SerializeField] [Range(0, 0.1f)] private float _shootingPhaseDistort;

		[FormerlySerializedAs("HookAssetPrefab"), SerializeField] private Transform _hookAssetPrefab;
		private Transform _hookAssetRef;

		public override Transform HookAsset
		{
			get
			{
				if (_hookAssetRef == null)
					_hookAssetRef = (Instantiate(_hookAssetPrefab, Vector3.zero, Quaternion.identity));
				return _hookAssetRef;
			}
		}

		// runs within [0,1] during rope shooting , is exactly 1 at every moment of the normal rope lifetime -- on decativated rope undefined
		private float rel_time(float start, float curr)
		{
			return MF.Clamp01((curr - start) / _shootingDuration);
		}

		private void UpdateHookAssetPos(float startT, float currT)
		{
			HookAsset.position = Vector3.Lerp(SpawnBeamAnchor.position, _targetTr.position,
				rel_time(startT, currT));
		}

		[Header("comes from Connect()", order = 1)]
		[Header("setting this in Editor is only useful for interactive Testing", order = 2)]
		private Transform _targetTr;

		private InterpolatedVelocities _intpv = new InterpolatedVelocities(10);

		public LineRenderer lineRenderer;
		
		public void Start()
		{
			InitMemberFromBalancingConfig();
			Disconnect();

			if (!TryGetComponent(out lineRenderer))
				lineRenderer = gameObject.AddComponent<LineRenderer>();
		}

		public void FixedUpdate()
		{
			if (!_visible)
				return;

			if (!_targetTr)
			{
				Disconnect();
				return;
			} // <- Hack for SceneReloading while Rope is active - in this case the HookTr GameObject is killed by the Scenemanager without warning


			RPI.UpdateTransforms(gunTr: SpawnBeamAnchor, hookTr: HookAsset, conf: Conf);
			InternalUpdate(_startTime, Time.time, Time.fixedDeltaTime);

			// controller velocity stuff
			_intpv.Push(_velocityRefPoint.position, Time.time);
		}

		public void LateUpdate()
		{
			if (!_visible)
				return;

			if (!_targetTr)
			{
				Disconnect();
				return;
			} // <- Hack for SceneReloading while Rope is active - in this case the HookTr GameObject is killed by the Scene manager without warning

			UpdateHookAssetPos(_startTime, Time.time);
			RPI.UpdateTransforms(gunTr: SpawnBeamAnchor, hookTr: HookAsset, conf: Conf);

			Conf.Splinalize(RPI);

			if (IsConnected)
			{
				lineRenderer.positionCount = RPI.CurrentN;
				lineRenderer.SetPositions(RPI.Ps);
			}
			// Micha: trigger Collision detection  -> removed because of Nico's Feedback (let's see what comes next ;)
			//CheckForRopeCollision();
		}
		// -------------------------------

		public bool ConeVeloTest(float thresholdV)
		{
			Vector3 projDirection = (_targetTr.position - _velocityRefPoint.position).normalized;

			foreach (Vector3 v in _intpv.RelativeVelos(_velocityRefPoint.position, Time.time))
			{
				float cosa = Vector3.Dot(v.normalized, -projDirection);
				float ang = Mathf.Acos(Mathf.Clamp(cosa, -1, 1));
				float coneVelo = 0;
				if ((ang > 0) && (ang <= _angularConeRad))
				{
					coneVelo = v.magnitude;
				}

				if (coneVelo > thresholdV)
					return true;
			}

			return false;
		}

		public void InternalUpdate(float startTime, float currentTime, float dt)
		{
			if (_shootingPhase)
			{
				ShootingPhaseSpawn(startTime, currentTime, dt);
				_fadedStiffness = 0.9f;
				Conf.T = _fadedStiffness;
			}
			else
			{
				_directStiffness = StiffnessFactorDirect();
				FadeStiffnessCurve();
				Conf.T = _fadedStiffness;

				OnTensionValueChanged(_fadedStiffness);

				if (ConeVeloTest(_upperVelo))
					TriggerTeleport();
			}

			Conf.FrameUpdatePhys(RPI, dt);
			// shooting phase flanke
			if (_shootingPhase && (currentTime - startTime) > _shootingDuration)
			{
				OnRolledOut();

				_shootingPhase = false;
			}
		}

		// operates under the assumption, that it is called at least _once_ with a time value such that :
		// (currentTime - StartTime   ) >= shootingDuration

		public void ShootingPhaseSpawn(float startTime, float currentTime, float dt = 0.11111f)
		{
			Vector3 gunPos = SpawnBeamAnchor.position;
			float relTime = rel_time(startTime, currentTime);
			Vector3 hookPos = Vector3.Lerp(gunPos, _targetTr.position, rel_time(_startTime, currentTime));
			float targetDist = (gunPos - hookPos).magnitude;


			// spawn_hack_l{0,1} , spawn_hack_l{0,1}_no_CPs define a linear function ( rope_length -> number_of_spline_control_points )
			float spawnHackLenFac = Mathf.Clamp01((targetDist - _spawnHackL0) / (_spawnHackL1 - _spawnHackL0));
			int desiredElementCount =
				(int)(_spawnHackL0NoCPs +
					   spawnHackLenFac *
					   (_spawnHackL1NoCPs -
						_spawnHackL0NoCPs)); // unrolled 2x2 affine transform between these spaces

			int newCpCount = Math.Max(0, desiredElementCount - RPI.CurrentN);

			/*
                Ex: spwan two new phys points in a single frame ( O old CPs , X new CPs )  

                Anchor                                         Gun  
                          |O--------O---------X-----X--------O|
                CP_index  0                                  Tail_I
            */
			Func<float> r1 = () => UnityEngine.Random.Range(-1, 1);
			Func<Quaternion> randQ = () => {
				var v4 = new Vector4(r1(), r1(), r1(), r1());
				v4.Normalize();
				return new Quaternion(v4.x, v4.y, v4.z, v4.w);
			};

			if (newCpCount > 0)
			{
				float d = 1.0f / (newCpCount + 1);

				Vector3 nHookP = RPI.Ps[1];
				Vector3 tailP = RPI.Ps[RPI.TailI];
				for (int i = 0; i < newCpCount; i++)
				{
					Vector3 nuPos = Vector3.Lerp(nHookP, hookPos, (i + 1) * d);
					RPI.PushCP_anchor_segment(ref nuPos);
					Quaternion rot = Quaternion.FromToRotation(Vector3.forward, tailP - hookPos);

					RPI.Rots[RPI.TailI - 1] = Quaternion.Slerp(rot, randQ(), _shootingPhaseDistort);
					// real ugly hack to make the system converge faster
					RPI.Ps[0] = hookPos;
				}
			}

			Conf.AdaptRestLength((hookPos - gunPos).magnitude * relTime,
				RPI); // <-- adapt all the time during shooting, because the number of phys points varies
		}

		public float StiffnessFactorDirect()
		{
			float delta = Mathf.Clamp(_intpv.ImmediateVelo().magnitude, _lowerVelo, _upperVelo);
			delta = (delta - _lowerVelo) / (_upperVelo - _lowerVelo);
			//delta.NLSend("stiffness factor");
			return delta;
		}

		public void FadeStiffnessCurve()
		{
			// i don't really care
			// whenever the measured value surpasses the hysteresis use that new value as scaling factor and start the entire process over
			// without this hack, the inverse of an AnimationCurve would be needed
			if (_directStiffness > _fadedStiffness)
			{
				_stiffnessDelayStartT = Time.time;
				_fadedStiffness = _directStiffness;
				_stiffnessDelayAnchor = _directStiffness;
			}
			else
			{
				var normalizedDeltaT = (Time.time - _stiffnessDelayStartT) / _stiffnessFadeDelay;
				_fadedStiffness = _stiffnessDelayAnchor * _stiffnessFadeCurve.Evaluate(normalizedDeltaT);
			}
		}
		
		// --- Interface ---

		// -- public bool teleportTriggered --

		public override void Connect(Chargeable target)
		{
			base.Connect(target);
			Transform targetTransform = target.AnchorTransform;

			//if (!targetTransform) throw new Exception("Connect with null Target");
			if (targetTransform == null)
			{
				Debug.LogError("Can't connect with null Target!");
				return;
			}

			_visible = true;
			_shootingPhase = true;
			_targetTr = targetTransform;
			_startTime = Time.time;
			Conf.T = 1;

			HookAsset.gameObject.SetActive(true);
			var position = transform.position;
			HookAsset.position = position;
			HookAsset.LookAt(position - SpawnBeamAnchor.forward);

			RPI.HardReset(Conf, SpawnBeamAnchor, HookAsset);

			lineRenderer.enabled = true;
			// da ich keinen Schimmer hab, in welcher Phase das connect aufgerufen wird :
			InternalUpdate(_startTime, _startTime, 0.03f);
			UpdateChargeValue(target.CurrentCharge.value);
		}
		
		public override void UpdateChargeValue(float currentCharge)
		{
			lineRenderer.material.SetFloat("_Fill",currentCharge);
		}

		/*
            this also doubles as a ResetFunction -> idempotent, can be called any number of times , any time - just to make sure 
            a Disconnect() is automatically triggered whenever the "Target" Transform passed to Connect() is discovered to 
            have become null ( as happens at SceneReload ) 
        */
		public override void Disconnect()
		{
			base.Disconnect();
			_visible = false;

			lineRenderer.SetPositions(new Vector3[] { });
			lineRenderer.enabled = false;
			HookAsset.gameObject.SetActive(false);

			_targetTr = null;
			_intpv.Reset();
		}

		public override Transform SpawnBeamAnchor { get { return _spawnBeamAnchor; } set { _spawnBeamAnchor = value; } }

		public override void OnTeamChanged(IPlayer player, TeamID teamID)
		{
			ITeam team = TeamManager.Singleton.Get(teamID);

			if (team == null)
			{
				Debug.LogWarning("RopeChargerBeamRendererTess.OnTeamChanged: Team is not valid!");
				return;
			}

			lineRenderer.material.SetColor("_TintColor", team.Colors.Rope);
		}

		void InitMemberFromBalancingConfig()
		{
			_upperVelo = BalancingConfiguration.Singleton.TeleportTriggerSpeed;
		}
		// Micha: handle Collisions - End

		private void OnDestroy()
		{
			if (_hookAssetRef != null)
				Destroy(_hookAssetRef.gameObject);
		}
	}
}