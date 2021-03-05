using System;
using System.Linq;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace AI {
    /// <summary>
    /// Conditional Task that selects a <see cref="Pillar"/> in the vicinity of the bot,
    /// based on a score that is determined by a set of parameters, such as the closeness
    /// to enemies or other pillars.
    /// If a pillar is found, returns success and stores the pillar top in <see cref="_target"/>
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TargetPillar : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private SharedVector3 _target;

        [SerializeField, BehaviorDesigner.Runtime.Tasks.Tooltip("Pillar score must be better than current pillar")]
        private bool _mustBeBetterThanCurrent;

        [SerializeField, BehaviorDesigner.Runtime.Tasks.Tooltip("Is Bot in fighting sequence")]
        private bool _isInFightSequence;

        [SerializeField, BehaviorDesigner.Runtime.Tasks.Tooltip("Is Bot being shot at and hiding")]
        private bool _isInHideSequence;

        [SerializeField, BehaviorDesigner.Runtime.Tasks.Tooltip("Bonus score for team claimed pillars")]
        private float _claimedPillarScore;

        [SerializeField, BehaviorDesigner.Runtime.Tasks.Tooltip("Penalty score for pillars surrounded by >=2 enemies")]
        private float _surroundedPillarScore;

        [SerializeField, BehaviorDesigner.Runtime.Tasks.Tooltip("Enemy Attractiveness if only bots left")]
        private float _botMatchEnemyAttractiveness = 100f;

        [SerializeField] private float _randomScore;


        private IPlayer Player => _botBrain.Value.Player;
        private Pillar Pillar => _botBrain.Value.TargetPillar;
        private Pillar _chosenPillar;

        public override TaskStatus OnUpdate() {
            if (Player == null)
                return TaskStatus.Failure;
            if (Player.CurrentPillar == null)
                return TaskStatus.Failure;
            _chosenPillar = PillarManager.Instance.GetNeighboursByPlayer(Player)
                .Where(pillar => pillar != null)
                .Where(neighbourPillar => neighbourPillar.ID != Player.CurrentPillar.ID)
                .Where(neighbourPillar => !neighbourPillar.IsOccupied)
                .Where(CanClaimPillar)
                .OrderByDescending(PillarScore)
                .FirstOrDefault();

            if (_chosenPillar == null)
                return TaskStatus.Failure;
            if (_mustBeBetterThanCurrent && PillarScore(_chosenPillar) < PillarScore(Player.CurrentPillar))
                return TaskStatus.Failure;

            if (Pillar != _chosenPillar && Player.HasRopeAttached)
                Player.GunController.RequestRopeDisconnect(true);

            _botBrain.Value.TargetPillar = _chosenPillar;
            _target.SetValue(_chosenPillar.AnchorTransform.position);
            return TaskStatus.Success;
        }

        private float PillarScore(Pillar pillar) {
            if (_isInFightSequence)
                return PillarScoreFighting(pillar);
            else if (_isInHideSequence)
                return PillarScoreHiding(pillar);
            return PillarScoreScouting(pillar);
        }

        private bool CanClaimPillar(Pillar pillar) {
            if (!pillar.CanAttach(_botBrain.Value.Player))
                return false;

            if (_isInHideSequence)
                return CanClaimPillarWhenHiding(pillar);
            else
                return CanClaimPillarFromHere(pillar);
        }


        /// <summary>
        /// pillar score during scouting "Search & Claim" sequence
        /// takes enemy spawn, enemy position and a random value into account
        /// </summary>
        private float PillarScoreScouting(Pillar pillar) {
            float score = 0;
            score += EnemyBaseDistanceScore(pillar);
            score += EnemyDistanceScore(pillar);
            score += PillarSurroundedScore(pillar);
            score += Random.Range(0, _randomScore);

            return score;
        }


        /// <summary>
        /// pillar score during fighting sequence
        /// takes enemy position and a random value into account
        /// </summary>
        /// <param name="pillar"></param>
        /// <returns></returns>
        private float PillarScoreFighting(Pillar pillar) {
            float score = 0;
            score += EnemyDistanceScore(pillar);
            score += PillarSurroundedScore(pillar);
            score += Random.Range(0, _randomScore);

            return score;
        }


        /// <summary>
        /// pillar score during hide & jump sequence
        /// takes enemy position, pillar claim status and a random value into account
        /// </summary>
        /// <param name="pillar"></param>
        /// <returns></returns>
        private float PillarScoreHiding(Pillar pillar) {
            float score = 0;
            score -= EnemyDistanceScore(pillar);
            score += PillarClaimedScore(pillar); //prefer pillars that are already claimed
            score += PillarSurroundedScore(pillar);
            score += Random.Range(0, _randomScore);

            return score;
        }


        private bool CanClaimPillarFromHere(Pillar pillar) {
            Pillar currentPillar = Player.CurrentPillar;
            if (currentPillar == null) return false;
            Vector3 rayOrigin = currentPillar.transform.position + 2f * Vector3.up;
            Vector3 rayDirection = pillar.AnchorTransform.position - rayOrigin;
            // todo layer mask to prevent blocking raycast with own head etc.
            return Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo) &&
                   pillar.ChargeableCollider.Contains(hitInfo.collider.GetComponent<ChargeableCollider>());
        }


        private bool CanClaimPillarWhenHiding(Pillar pillar) {
            GameObject player = Player.GameObject;
            if (player == null) return false;
            Vector3 rayOrigin = player.transform.position;
            Vector3 rayDirection = pillar.AnchorTransform.position - rayOrigin;
            //layer mask is used to prevent blocking raycast with own head avatar etc.
            return Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, _botBrain.Value.ClaimLayerMask) &&
                   pillar.ChargeableCollider.Contains(hitInfo.collider.GetComponent<ChargeableCollider>());
        }


        private float EnemyDistanceScore(Pillar pillar) {
            float score = 0;

            IPlayer closestEnemy = _botBrain.Value.ClosestEnemy();

            if (OnlyBotsLeft() && closestEnemy != null && closestEnemy.GameObject != null) {
                return -Vector3.Distance(pillar.transform.position, closestEnemy.GameObject.transform.position) *
                       _botMatchEnemyAttractiveness;
            }

            if (closestEnemy != null && closestEnemy.GameObject != null) {
                score -= Vector3.Distance(pillar.transform.position, closestEnemy.GameObject.transform.position)
                         * _botBrain.Value.AIParameters.EnemyAttractiveness;
            }

            return score;
        }

        private float EnemyBaseDistanceScore(Pillar pillar) {
            float score = 0;

            Pillar enemySpawnPillar =
                PillarManager.Instance.FindSpawnPillar(Player.TeamID == TeamID.Fire
                    ? TeamID.Ice
                    : TeamID.Fire);
            if (enemySpawnPillar != null) {
                score -= Vector3.Distance(pillar.transform.position, enemySpawnPillar.transform.position)
                         * _botBrain.Value.AIParameters.EnemyBaseAttractiveness;
            }

            return score;
        }

        private float PillarClaimedScore(Pillar pillar) {
            float score = 0;

            if (pillar.CanTeleport(Player)) {
                score += _claimedPillarScore;
            }

            return score;
        }

        private float PillarSurroundedScore(Pillar pillar) {
            float score = 0;
            float enemyCount = 0;
            Pillar[] neighbourPillars = PillarManager.Instance.GetNeighboursByPillarID(pillar.ID);
            if (neighbourPillars.Length <= 1)
                return score; // only one possible enemy in pillar neighbourhood
            foreach (Pillar neighbourPillar in neighbourPillars) {
                if (neighbourPillar.OwningTeamID != Player.TeamID && neighbourPillar.IsOccupied
                ) //if a neighbour pillar is occupied by an enemy
                {
                    enemyCount++;
                }
            }

            if (enemyCount >= 2)
                score += _surroundedPillarScore; //if there are >= 2 enemies in the neighbourhood, add penalty score

            return score;
        }

        private bool OnlyBotsLeft() {
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            return !players.Take(count).Any(player => player.PlayerHealth.IsAlive && !player.IsBot);
        }
    }
}