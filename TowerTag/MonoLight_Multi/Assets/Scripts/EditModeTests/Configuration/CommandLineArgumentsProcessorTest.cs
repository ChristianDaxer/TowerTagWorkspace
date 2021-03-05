using System.Text.RegularExpressions;
using NUnit.Framework;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class CommandLineArgumentsProcessorTest {
    [TearDown]
    public void TearDown() {
        foreach (var singleton in Resources.FindObjectsOfTypeAll<SharedControllerType>()) {
            if (UnityEditor.AssetDatabase.Contains(singleton)) {
                Resources.UnloadAsset(singleton);
            }
            else {
                Object.DestroyImmediate(singleton);
            }
        }
    }
    
    [Test]
    public void ShouldSetControllerTypeSpectator() {
        // GIVEN a controller type asset
        var sharedControllerType = ScriptableObject.CreateInstance<SharedControllerType>();

        // WHEN processing arguments that include 'vr'
        CommandLineArgumentsProcessor.ProcessCommandLineArguments(new[] {"test", "-spectator"}, sharedControllerType);

        // THEN controller type should be vr
        Assert.AreEqual(ControllerType.Spectator, sharedControllerType.Value);
    }
    
    [Test]
    public void ShouldSetControllerTypeVR() {
        // GIVEN a controller type asset
        var sharedControllerType = ScriptableObject.CreateInstance<SharedControllerType>();

        // WHEN processing arguments that include 'vr'
        CommandLineArgumentsProcessor.ProcessCommandLineArguments(new[] {"test", "-vr"}, sharedControllerType);

        // THEN controller type should be vr
        Assert.AreEqual(ControllerType.VR, sharedControllerType.Value);
    }

    [Test]
    public void ShouldSetControllerTypeAdmin() {
        // GIVEN a controller type asset
        var sharedControllerType = ScriptableObject.CreateInstance<SharedControllerType>();

        // WHEN processing arguments that include 'admin'
        CommandLineArgumentsProcessor.ProcessCommandLineArguments(new[] {"test", "-admin"}, sharedControllerType);

        // THEN controller type should be admin
        Assert.AreEqual(ControllerType.Admin, sharedControllerType.Value);
    }

    [Test]
    public void ShouldSetControllerTypeFPS() {
        // GIVEN a controller type asset
        var sharedControllerType = ScriptableObject.CreateInstance<SharedControllerType>();

        // WHEN processing arguments that include 'fps'
        CommandLineArgumentsProcessor.ProcessCommandLineArguments(new[] {"test", "-fps"}, sharedControllerType);

        // THEN controller type should be fps
        Assert.AreEqual(ControllerType.NormalFPS, sharedControllerType.Value);
    }

    [Test]
    public void ShouldSetControllerTypePOController() {
        // GIVEN a controller type asset
        var sharedControllerType = ScriptableObject.CreateInstance<SharedControllerType>();

        // WHEN processing arguments that include 'po controller'
        CommandLineArgumentsProcessor.ProcessCommandLineArguments(new[] {"test", "-poCtrlr"}, sharedControllerType);

        // THEN controller type should be po controller
        Assert.AreEqual(ControllerType.PillarOffsetController, sharedControllerType.Value);
    }

    [Test]
    public void ShouldLogWarningForConflictingSettings() {
        // GIVEN a controller type asset
        var sharedControllerType = ScriptableObject.CreateInstance<SharedControllerType>();

        // WHEN processing arguments that include 'ai' and 'vr'
        CommandLineArgumentsProcessor.ProcessCommandLineArguments(new[] {"-admin", "test", "-vr"}, sharedControllerType);

        // THEN controller type should be vr, because it was passed last
        Assert.AreEqual(ControllerType.VR, sharedControllerType.Value);
        LogAssert.Expect(LogType.Warning, new Regex(".*Conflicting settings for controller type"));
    }
}