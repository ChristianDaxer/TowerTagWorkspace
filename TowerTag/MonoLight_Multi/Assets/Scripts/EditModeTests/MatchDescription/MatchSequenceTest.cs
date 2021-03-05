using System.Reflection;
using NUnit.Framework;
using UnityEditor;

/// <summary>
/// Test the map sequence asset that is used to choose the next map in basic mode.
/// This test hard codes expected sequences and must be adapted, when maps are added to or removed from the sequence.
/// </summary>
public class MatchSequenceTest {
    private const string AssetPath = "Assets/ScriptableObjects/MatchDescriptions/BasicMatchSequence.asset";
    private MatchSequence _matchSequence;

    [SetUp]
    public void SetUp() {
        _matchSequence = AssetDatabase.LoadAssetAtPath<MatchSequence>(AssetPath);
        FieldInfo indexField = _matchSequence.GetType()
            .GetField("_index", BindingFlags.Instance | BindingFlags.NonPublic);
        if (indexField != null) indexField.SetValue(_matchSequence, 0);
    }

    [Test]
    public void ShouldFindAsset() {
        var matchSequence = AssetDatabase.LoadAssetAtPath<MatchSequence>(AssetPath);
        Assert.NotNull(matchSequence);
    }

    [Test]
    public void ShouldReturnNullForOnePlayer() {
        MatchDescription matchDescription = _matchSequence.Next(1);
        Assert.Null(matchDescription);
    }

    [Test]
    public void ShouldReturnNullForNinePlayers() {
        MatchDescription matchDescription = _matchSequence.Next(9);
        Assert.Null(matchDescription);
    }

    [Test]
    public void ShouldReturnSequenceForTwoPlayers() {
        Assert.AreEqual("Cebitus", _matchSequence.Next(2).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(2).MapName);

        Assert.AreEqual("Cebitus", _matchSequence.Next(2).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(2).MapName);
    }

    [Test]
    public void ShouldReturnSequenceForThreePlayers() {
        Assert.AreEqual("Cebitus", _matchSequence.Next(3).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(3).MapName);

        Assert.AreEqual("Cebitus", _matchSequence.Next(3).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(3).MapName);
    }

    [Test]
    public void ShouldReturnSequenceForFourPlayers() {
        Assert.AreEqual("Cebitus", _matchSequence.Next(4).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(4).MapName);
        Assert.AreEqual("Millerntor", _matchSequence.Next(4).MapName);
        Assert.AreEqual("Millerntor", _matchSequence.Next(4).MapName);

        Assert.AreEqual("Cebitus", _matchSequence.Next(4).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(4).MapName);
        Assert.AreEqual("Millerntor", _matchSequence.Next(4).MapName);
        Assert.AreEqual("Millerntor", _matchSequence.Next(4).MapName);
    }

    [Test]
    public void ShouldReturnSequenceForFivePlayers() {
        Assert.AreEqual("Cebitus", _matchSequence.Next(5).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(5).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(5).MapName);
        Assert.AreEqual("Dome", _matchSequence.Next(5).MapName);

        Assert.AreEqual("Cebitus", _matchSequence.Next(5).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(5).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(5).MapName);
        Assert.AreEqual("Dome", _matchSequence.Next(5).MapName);
    }

    [Test]
    public void ShouldReturnSequenceForSixPlayers() {
        Assert.AreEqual("Cebitus", _matchSequence.Next(6).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(6).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(6).MapName);
        Assert.AreEqual("Dome", _matchSequence.Next(6).MapName);

        Assert.AreEqual("Cebitus", _matchSequence.Next(6).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(6).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(6).MapName);
        Assert.AreEqual("Dome", _matchSequence.Next(6).MapName);
    }

    [Test]
    public void ShouldReturnSequenceForSevenPlayers() {
        Assert.AreEqual("Shield", _matchSequence.Next(7).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(7).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(7).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(7).MapName);

        Assert.AreEqual("Shield", _matchSequence.Next(7).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(7).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(7).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(7).MapName);
    }

    [Test]
    public void ShouldReturnSequenceForEightPlayers() {
        Assert.AreEqual("Shield", _matchSequence.Next(8).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(8).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(8).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(8).MapName);

        Assert.AreEqual("Shield", _matchSequence.Next(8).MapName);
        Assert.AreEqual("Shield", _matchSequence.Next(8).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(8).MapName);
        Assert.AreEqual("Sneaky", _matchSequence.Next(8).MapName);
    }
}