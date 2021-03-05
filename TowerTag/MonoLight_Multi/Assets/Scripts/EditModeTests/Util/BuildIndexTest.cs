using NUnit.Framework;
using UnityEditor;
using UnityEngine.SceneManagement;

public class BuildIndexTest {
    [Test]
    public void ShouldStartInLicenseActivationScene() {
        string[] activeSceneList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        Assert.AreEqual("Assets/Scenes/GameScenes/License Activation.unity", activeSceneList[0]);
    }
}