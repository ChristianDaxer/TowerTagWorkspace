#if !UNITY_ANDROID
using System.Collections;
using System.Text;
using Cryptlex;
using GameManagement;
using NSubstitute;
using TowerTagSOES;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests {
    public class LicenseIntegrationTest : TowerTagTest {
        [UnityTest]
        public IEnumerator ShouldCheckLicense() {
            // GIVEN: a cryptlex service mock
            var cryptlexService = Substitute.For<ICryptlexService>();
            ServiceProvider.Set(cryptlexService);
            // and an FPS controller type
            SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

            // WHEN loading the entry point scene and waiting a frame
            yield return SceneManager.LoadSceneAsync(0);
            yield return null;

            // THEN a license check should have happened
            cryptlexService.Received().IsLicenseGenuine();
        }

        [UnityTest]
        public IEnumerator ShouldPassLicenseCheckAndContinueToMenuScene() {
            // GIVEN: a cryptlex service mock
            var cryptlexService = Substitute.For<ICryptlexService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_OK);
            ServiceProvider.Set(cryptlexService);
            // and a scene service mock
            var sceneService = Substitute.For<ISceneService>();
            ServiceProvider.Set(sceneService);
            // and an FPS controller type
            SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

            // WHEN loading the entry point scene and waiting a frame
            yield return SceneManager.LoadSceneAsync(0);
            yield return null;

            // THEN should load connection scene
            sceneService.Received().LoadConnectScene(Arg.Any<bool>());
        }

        [UnityTest]
        public IEnumerator ShouldPassLicenseCheckAndConnectWithAutoStart() {
            // GIVEN: a cryptlex service mock
            var cryptlexService = Substitute.For<ICryptlexService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_OK);
            ServiceProvider.Set(cryptlexService);
            // and the actual scene service
            var sceneService = ServiceProvider.Get<ISceneService>();
            // and a mock photon service
            var photonService = Substitute.For<IPhotonService>();
            ServiceProvider.Set(photonService);
            // and an FPS controller type
            SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

            // WHEN loading the entry point scene and waiting a frame
            bool autoStart = BalancingConfiguration.Singleton.AutoStart;
            BalancingConfiguration.Singleton.AutoStart = true;
            yield return SceneManager.LoadSceneAsync(0);
            yield return new WaitUntil(() => sceneService.IsInConnectScene);
            yield return null;
            BalancingConfiguration.Singleton.AutoStart = autoStart;

            // THEN should connect
            photonService.Received().ConnectUsingSettings();
        }

        [UnityTest]
        public IEnumerator ShouldFailLicenseCheckAndNotConnectWithAutoStart() {
            // GIVEN: a cryptlex service mock
            var cryptlexService = Substitute.For<ICryptlexService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_FAIL);
            cryptlexService.IsProductGenuine().Returns(StatusCodesV2.LA_FAIL);
            ServiceProvider.Set(cryptlexService);
            // and a scene service mock
            var sceneService = Substitute.For<ISceneService>();
            ServiceProvider.Set(sceneService);
            // and a mock photon service
            var photonService = Substitute.For<IPhotonService>();
            ServiceProvider.Set(photonService);
            // and an FPS controller type
            SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

            // WHEN loading the entry point scene with auto start setting and waiting a frame
            bool autoStart = BalancingConfiguration.Singleton.AutoStart;
            BalancingConfiguration.Singleton.AutoStart = true;
            yield return SceneManager.LoadSceneAsync(0);
            yield return null;
            BalancingConfiguration.Singleton.AutoStart = autoStart;

            // THEN should not load connection scene
            sceneService.DidNotReceive().LoadConnectScene(Arg.Any<bool>());
            photonService.DidNotReceive().ConnectUsingSettings();
        }

        [UnityTest]
        public IEnumerator ShouldFailLicenseCheckAndNotContinueToMenuScene() {
            // GIVEN: a cryptlex service mock
            var cryptlexService = Substitute.For<ICryptlexService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_FAIL);
            cryptlexService.IsProductGenuine().Returns(StatusCodesV2.LA_FAIL);
            ServiceProvider.Set(cryptlexService);
            // and a scene service mock
            var sceneService = Substitute.For<ISceneService>();
            ServiceProvider.Set(sceneService);
            // and a mock photon service
            var photonService = Substitute.For<IPhotonService>();
            ServiceProvider.Set(photonService);
            // and an FPS controller type
            SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

            // WHEN loading the entry point scene and waiting a frame
            yield return SceneManager.LoadSceneAsync(0);
            yield return null;

            // THEN should not load connection scene
            sceneService.DidNotReceive().LoadConnectScene(Arg.Any<bool>());
            photonService.DidNotReceive().ConnectUsingSettings();
        }

        [UnityTest]
        public IEnumerator ShouldPassAfterGracePeriodExpired() {
            // GIVEN: a cryptlex service mock
            var cryptlexService = Substitute.For<ICryptlexService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_GRACE_PERIOD_OVER);
            cryptlexService.IsProductGenuine().Returns(StatusCodesV2.LA_EXPIRED, StatusCodesV2.LA_FAIL);
            cryptlexService.GetLicenseKey(Arg.Any<StringBuilder>(), Arg.Any<int>()).Returns(StatusCodes.LA_FAIL);
            cryptlexService.GetProductKey(Arg.Any<StringBuilder>(), Arg.Any<int>()).Returns(StatusCodesV2.LA_FAIL);
            ServiceProvider.Set(cryptlexService);
            // and a scene service mock
            var sceneService = Substitute.For<ISceneService>();
            sceneService.When(x => x.LoadConnectScene(true)).Do(x => TTSceneManager.Instance.LoadConnectScene(true));
            ServiceProvider.Set(sceneService);
            // and a message queue service mock
            var messageQueueService = Substitute.For<IMessageQueueService>();
            ServiceProvider.Set(messageQueueService);
            // and an FPS controller type
            SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

            // WHEN loading the entry point scene and waiting a frame
            yield return SceneManager.LoadSceneAsync(0);
            yield return null;

            // THEN should reactivate and load connection scene
            cryptlexService.Received().ActivateLicense();
            sceneService.Received().LoadConnectScene(Arg.Any<bool>());

            messageQueueService.DidNotReceive().AddErrorMessage(Arg.Any<string>());
        }
    }
}
#endif