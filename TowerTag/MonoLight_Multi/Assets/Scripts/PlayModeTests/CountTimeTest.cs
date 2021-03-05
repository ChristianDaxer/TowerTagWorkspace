using System.Collections;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
    public class CountTimeTest {
        [UnityTest]
        public IEnumerator CountTimeTestWithEnumeratorPasses() {
            // GIVEN a time service that always returns 7 for frame time
            var fake = Substitute.For<ITimeService>();
            fake.DeltaTime.Returns(7);

            // a CountTime object
            var go = new GameObject();
            var countTime = go.AddComponent<CountTime>();
            // substitute time service using reflection, because we don't want to add a public setter just for testing
            typeof(CountTime).GetField("_timeService", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(countTime, fake);

            // WHEN waiting one frame
            yield return null;

            // THEN counter should count one frame time
            Assert.AreEqual(7, countTime.TimeTest);
        }
    }
}