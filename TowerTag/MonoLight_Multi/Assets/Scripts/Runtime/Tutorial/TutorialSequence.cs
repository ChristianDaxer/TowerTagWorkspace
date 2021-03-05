using System.Collections;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

namespace Tutorial {
    public abstract class TutorialSequence : ScriptableObject {
        protected IPlayer _ownPlayer => PlayerManager.Instance.GetOwnPlayer();
        public bool IsRunning { get; private set; }

        public void Init() {
            ResetValues();
            IsRunning = true;
            StaticCoroutine.StartStaticCoroutine(StartSequence());
        }

        public void Finish(TutorialSequence nextSequence) {
            IsRunning = false;
            StaticCoroutine.StartStaticCoroutine(EndSequence(nextSequence));
        }

        /// <summary>
        /// Reset all variables here or they will be saved in the scriptable object!
        /// </summary>
        protected abstract void ResetValues();

        /// <summary>
        /// The behaviour of the sequence when it gets initialized
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator StartSequence();

        /// <summary>
        /// The behaviour of the scene, when the conditions are true
        /// </summary>
        /// <param name="nextSequence">If not null the next sequence</param>
        /// <returns></returns>
        protected abstract IEnumerator EndSequence([CanBeNull] TutorialSequence nextSequence);

        /// <summary>
        /// Checks for the conditions and starts the end sequence when true
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Defines the conditions for the tutorial
        /// </summary>
        /// <returns>True if all conditions are true</returns>
        public abstract bool IsCompleted();
    }
}