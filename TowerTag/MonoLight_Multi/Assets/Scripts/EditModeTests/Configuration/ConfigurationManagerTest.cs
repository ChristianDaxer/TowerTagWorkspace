using System.IO;
using NUnit.Framework;
using UnityEngine;

public class ConfigurationManagerTest {
    [Test]
    public void ShouldWriteConfigurationToFile() {
        Assert.True(ConfigurationManager.WriteConfigToFile());
    }

    [Test]
    public void ShouldWriteAndReadConfigurationFromFile() {
        // GIVEN a configuration with certain settings written to a file
        ConfigurationManager.Configuration.AnnouncerVolume = 0.77f;
        ConfigurationManager.Configuration.Room = "room";
        ConfigurationManager.Configuration.PillarPositionOffset = new Vector3(1.23f, 0.12f, 7f);
        ConfigurationManager.WriteConfigToFile();

        // and other configurations active
        ConfigurationManager.Configuration.AnnouncerVolume = 0.23f;
        ConfigurationManager.Configuration.Room = "otherRoom";
        ConfigurationManager.Configuration.PillarPositionOffset = new Vector3(1.27f, 0.1f, 5f);

        // WHEN loading configuration from the file
        Assert.True(ConfigurationManager.LoadConfigFromFile());

        // THEN the configuration from the file should be active
        Assert.AreEqual(0.77f, ConfigurationManager.Configuration.AnnouncerVolume);
        Assert.AreEqual("room", ConfigurationManager.Configuration.Room);
        Assert.AreEqual(new Vector3(1.23f, 0.12f, 7f), ConfigurationManager.Configuration.PillarPositionOffset);
    }

    [OneTimeTearDown]
    public void TearDown() {
        File.Delete($"{Application.dataPath}/../Config.xml");
    }
}