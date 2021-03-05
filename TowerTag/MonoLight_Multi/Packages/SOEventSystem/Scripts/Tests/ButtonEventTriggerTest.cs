using System.Collections;
using NUnit.Framework;
using SOEventSystem.Listeners;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SOEventSystem.Tests {
    public class ButtonEventTriggerTest {
        [UnityTest]
        public IEnumerator ShouldRequireButtonComponent() {
            // GIVEN a gameobject with an added ButtonEventTrigger
            var buttonEventTrigger = new GameObject().AddComponent<ButtonEventTrigger>();

            // WHEN waiting a frame
            yield return null;

            // THEN the gameobject should also have a button component
            Assert.NotNull(buttonEventTrigger.GetComponent<Button>());
        }

        [UnityTest]
        public IEnumerator ShouldTriggerSharedEventOnClick() {
            // GIVEN a ButtonEventTrigger and its button
            var buttonEventTrigger = new GameObject().AddComponent<ButtonEventTrigger>();
            var button = buttonEventTrigger.GetComponent<Button>();
            // and a shared event that registers when its triggered
            int invocations = 0;
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            sharedEvent.Triggered += sender => {
                Assert.AreEqual(buttonEventTrigger, sender, "Invoked by the wrong sender");
                invocations++;
            };
            buttonEventTrigger.SharedEvent = sharedEvent;

            // WHEN waiting a frame and then triggering the button
            yield return null;
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);

            // THEN
            Assert.AreEqual(1, invocations, "Should have been invoked once");
        }

        [UnityTest]
        public IEnumerator ShouldNotThrowWithoutSharedEvent() {
            // GIVEN a ButtonEventTrigger and its button
            var buttonEventTrigger = new GameObject().AddComponent<ButtonEventTrigger>();
            var button = buttonEventTrigger.GetComponent<Button>();
            // and no shared event
            buttonEventTrigger.SharedEvent = null;

            // WHEN triggering the button
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return null;

            // THEN
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ShouldNotTriggerEventAfterItWasUnassigned() {
            // GIVEN a ButtonEventTrigger and its button
            var buttonEventTrigger = new GameObject().AddComponent<ButtonEventTrigger>();
            var button = buttonEventTrigger.GetComponent<Button>();
            // and a shared event that registers when its triggered
            int invocations = 0;
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            sharedEvent.Triggered += sender => { invocations++; };
            buttonEventTrigger.SharedEvent = sharedEvent;

            // WHEN waiting a frame, removing the shared event from the trigger, and then triggering the button
            yield return null;
            buttonEventTrigger.SharedEvent = null;
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);

            // THEN
            Assert.AreEqual(0, invocations, "Should not have triggered");
        }

        [UnityTest]
        public IEnumerator ShouldNotTriggerEventAfterANewOneWasAssigned() {
            // GIVEN a ButtonEventTrigger and its button
            var buttonEventTrigger = new GameObject().AddComponent<ButtonEventTrigger>();
            var button = buttonEventTrigger.GetComponent<Button>();

            // and a shared event that registers when its triggered
            int invocations = 0;
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            sharedEvent.Triggered += sender => {
                Assert.AreEqual(buttonEventTrigger, sender);
                invocations++;
            };
            buttonEventTrigger.SharedEvent = sharedEvent;

            // WHEN waiting a frame, removing the shared event from the trigger, and then triggering the button
            yield return null;
            buttonEventTrigger.SharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return null;

            // THEN
            Assert.AreEqual(0, invocations, "Should not have triggered");
        }
    }
}