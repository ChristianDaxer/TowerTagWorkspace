using UnityEngine;

namespace SOEventSystem.Examples {
    public class TestSharedEventListener : MonoBehaviour {
        public void Listen(object sender) {
            Debug.Log(sender + " fired event");
        }

        public void ListenToBool(object sender, bool booly) {
            Debug.Log(sender + " passed bool " + booly);
        }

        public void ListenToInt(object sender, int inty) {
            Debug.Log(sender + " passed int " + inty);
        }

        public void ListenToFloat(object sender, float floaty) {
            Debug.Log(sender + " passed float " + floaty);
        }

        public void ListenToString(object sender, string stringy) {
            Debug.Log(sender + " passed string " + stringy);
        }

        public void ListenToVector3(object sender, Vector3 vector3) {
            Debug.Log(sender + " passed Vector3 " + vector3);
        }

        public void ListenToObject(object sender, Object obj) {
            Debug.Log(sender + " passed Object " + obj);
        }

        public void ListenToGeneric(object sender, object obj) {
            Debug.Log(sender + " passed " + obj);
        }
    }
}