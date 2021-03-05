using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRNerdsUtilities.Tests {
    public class ExtendedDebugTest {
        private const string TimestampPrefixPattern = @"\d\d:\d\d:\d\d\.\d\d\d: ";

        [Test]
        public void ShouldReturnLogger() {
            Assert.AreEqual(UnityEngine.Debug.unityLogger, Debug.unityLogger);
        }

        [Test]
        public void ShouldReturnIsDebugBuild() {
            Assert.AreEqual(UnityEngine.Debug.isDebugBuild, Debug.isDebugBuild);
        }

        [Test]
        public void ShouldReturnIsDeveloperConsoleVisible() {
            Assert.AreEqual(UnityEngine.Debug.developerConsoleVisible, Debug.developerConsoleVisible);
        }

        [UnityTest]
        public IEnumerator ShouldLogWithTimestamp() {
            Debug.Log("Test");
            LogAssert.Expect(LogType.Log, new Regex(TimestampPrefixPattern + "Test"));
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldLogWarningWithTimestamp() {
            Debug.LogWarning("Test");
            LogAssert.Expect(LogType.Warning, new Regex(TimestampPrefixPattern + "Test"));
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldLogErrorWithTimestamp() {
            Debug.LogError("Test");
            LogAssert.Expect(LogType.Error, new Regex(TimestampPrefixPattern + "Test"));
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldLogFormat() {
            Debug.LogFormat("test {0}", "xxx");
            LogAssert.Expect(LogType.Log, new Regex(TimestampPrefixPattern + "test xxx"));
            return null;
        }
    }
}