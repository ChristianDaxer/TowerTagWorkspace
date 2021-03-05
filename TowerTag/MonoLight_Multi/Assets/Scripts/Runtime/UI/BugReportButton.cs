using SOEventSystem.Shared;
using UnityEngine;

namespace UI {
    public class BugReportButton : MonoBehaviour {
        /*
        [SerializeField] private SharedBool _reportingBug;
        [SerializeField] private UserReportingScript _userReportingScript;
        */

        private void Start() {
            // _reportingBug.Set(this, false);
        }

        public void CreateBugReport() {
            // _reportingBug.Set(this, true);
            // Debug.Log("Start Creating User Report");
            // _userReportingScript.CreateUserReport();
        }

        public void CancelBugReport() {
            // _reportingBug.Set(this, false);
            // _userReportingScript.CancelUserReport();
        }

        public void SubmitBugReport() {
            // _reportingBug.Set(this, false);
            // _userReportingScript.SubmitUserReport();
        }
    }
}