#if !UNITY_ANDROID
using Cryptlex;
using GameManagement;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tools {
    public class LicenseManagerTest {
        [TearDown]
        public void TearDown() {
            ServiceProvider.Clear();
        }

        [Test]
        public void ShouldCheckV3License() {
            var licenseManager = new GameObject().AddComponent<LicenseManager>();
            var cryptlexService = Substitute.For<ICryptlexService>();
            ServiceProvider.Set(cryptlexService);
            ServiceProvider.Set(Substitute.For<ISceneService>());
            licenseManager.Init();
            licenseManager._cryptlexProducts = new[] {
                new LicenseManager.CryptlexProduct {
                    CryptlexVersion = LicenseManager.CryptlexVersion.V3,
                    FileName = "file name",
                    GUID = "guid"
                }
            };
            licenseManager.CheckLicense();
            cryptlexService.Received().IsLicenseGenuine();
        }

        [Test]
        public void ShouldCheckV2License() {
            var licenseManager = new GameObject().AddComponent<LicenseManager>();
            var cryptlexService = Substitute.For<ICryptlexService>();
            ServiceProvider.Set(cryptlexService);
            ServiceProvider.Set(Substitute.For<ISceneService>());
            licenseManager.Init();
            licenseManager._cryptlexProducts = new[] {
                new LicenseManager.CryptlexProduct {
                    CryptlexVersion = LicenseManager.CryptlexVersion.V2,
                    FileName = "file name",
                    GUID = "guid"
                }
            };
            licenseManager.CheckLicense();
            cryptlexService.Received().IsProductGenuine();
        }

        [Test]
        public void ShouldLoadSceneWhenLicenseValid() {
            var licenseManager = new GameObject().AddComponent<LicenseManager>();
            var cryptlexService = Substitute.For<ICryptlexService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_OK);
            ServiceProvider.Set(cryptlexService);
            var sceneService = Substitute.For<ISceneService>();
            ServiceProvider.Set(sceneService);
            licenseManager.Init();
            licenseManager._cryptlexProducts = new[] {
                new LicenseManager.CryptlexProduct {
                    CryptlexVersion = LicenseManager.CryptlexVersion.V3,
                    FileName = "file name",
                    GUID = "guid"
                }
            };
            licenseManager.CheckLicense();

            sceneService.Received().LoadConnectScene(Arg.Any<bool>());
        }

        [Test]
        public void ShouldNotLoadSceneWhenLicenseInvalid() {
            var licenseManager = new GameObject().AddComponent<LicenseManager>();
            var cryptlexService = Substitute.For<ICryptlexService>();
            ServiceProvider.Set(cryptlexService);
            var sceneService = Substitute.For<ISceneService>();
            cryptlexService.IsLicenseGenuine().Returns(StatusCodes.LA_FAIL);
            ServiceProvider.Set(sceneService);
            licenseManager.Init();
            licenseManager._cryptlexProducts = new[] {
                new LicenseManager.CryptlexProduct {
                    CryptlexVersion = LicenseManager.CryptlexVersion.V3,
                    FileName = "file name",
                    GUID = "guid"
                }
            };
            licenseManager.CheckLicense();

            sceneService.DidNotReceive().LoadConnectScene(Arg.Any<bool>());
        }
    }
}
#endif