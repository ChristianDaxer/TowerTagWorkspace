using System;
using System.Collections.Generic;
using Commendations;
using NSubstitute;
using NUnit.Framework;
using TowerTag;

public class CommendationServiceTest {
    [Test]
    public void ShouldWorkWithoutPlayers() {
        var playerManager = Substitute.For<IPlayerManager>();
        var commendationService = new CommendationService();
        commendationService.SetPlayerManager(playerManager);
        Dictionary<IPlayer, (ICommendation, int)> awardCommendations = commendationService.AwardCommendations(
            new IPerformanceBasedCommendation[0],
            new IPlayer[0],
            1,
            new EliminationMatchStats());

        Assert.NotNull(awardCommendations);
        Assert.AreEqual(0, awardCommendations.Count);
    }

    [Test]
    public void ShouldAwardCommendations() {
        // player manager with 3 players
        var player0 = Substitute.For<IPlayer>();
        var player1 = Substitute.For<IPlayer>();
        var player2 = Substitute.For<IPlayer>();
        var playerManager = Substitute.For<IPlayerManager>();
        playerManager.GetPlayer(0).Returns(player0);
        playerManager.GetPlayer(1).Returns(player1);
        playerManager.GetPlayer(2).Returns(player2);

        var commendationService = new CommendationService();
        commendationService.SetPlayerManager(playerManager);

        var commendation0 = Substitute.For<IPerformanceBasedCommendation>();
        var commendation1 = Substitute.For<IPerformanceBasedCommendation>();
        var commendation2 = Substitute.For<IPerformanceBasedCommendation>();
        IPerformanceBasedCommendation[] commendations = {
            commendation0,
            commendation1,
            commendation2,
        };

        var matchStats = Substitute.For<IMatchStats>();
        matchStats.GameMode.Returns(GameMode.Elimination);
        commendation0.GetBestPlayer(matchStats).Returns(0);
        commendation1.GetBestPlayer(matchStats).Returns(1);
        commendation2.GetBestPlayer(matchStats).Returns(2);

        Dictionary<IPlayer, (ICommendation commendation, int place)> awardCommendations =
            commendationService.AwardCommendations(
                commendations,
                new[] {player0, player1, player2},
                3,
                matchStats);

        Assert.NotNull(awardCommendations);
        Assert.AreEqual(3, awardCommendations.Count);
        Assert.IsTrue(awardCommendations.ContainsKey(player0));
        Assert.IsTrue(awardCommendations.ContainsKey(player1));
        Assert.IsTrue(awardCommendations.ContainsKey(player2));
        Assert.AreEqual(commendation0, awardCommendations[player0].commendation);
        Assert.AreEqual(commendation1, awardCommendations[player1].commendation);
        Assert.AreEqual(commendation2, awardCommendations[player2].commendation);
        Assert.AreEqual(0, awardCommendations[player0].place);
        Assert.AreEqual(1, awardCommendations[player1].place);
        Assert.AreEqual(2, awardCommendations[player2].place);
    }

    [Test]
    public void ShouldAwardDefaultCommendations() {
        // player manager with 3 players
        var player0 = Substitute.For<IPlayer>();
        var player1 = Substitute.For<IPlayer>();
        var player2 = Substitute.For<IPlayer>();
        var playerManager = Substitute.For<IPlayerManager>();
        playerManager.GetPlayer(0).Returns(player0);
        playerManager.GetPlayer(1).Returns(player1);
        playerManager.GetPlayer(2).Returns(player2);

        var commendationService = new CommendationService();
        commendationService.SetPlayerManager(playerManager);

        var commendation0 = Substitute.For<IPerformanceBasedCommendation>();
        var commendation1 = Substitute.For<IPerformanceBasedCommendation>();
        IPerformanceBasedCommendation[] commendations = {
            commendation0,
            commendation1
        };

        var matchStats = Substitute.For<IMatchStats>();
        matchStats.GameMode.Returns(GameMode.Elimination);
        commendation0.GetBestPlayer(matchStats).Returns(0);
        commendation1.GetBestPlayer(matchStats).Returns(-1);

        var defaultCommendation = Substitute.For<ICommendation>();
        Dictionary<IPlayer, (ICommendation commendation, int place)> awardCommendations =
            commendationService.AwardCommendations(
                commendations,
                new[] {player0, player1, player2},
                3,
                matchStats,
                defaultCommendation);

        Assert.NotNull(awardCommendations);
        Assert.AreEqual(3, awardCommendations.Count);
        Assert.IsTrue(awardCommendations.ContainsKey(player0));
        Assert.IsTrue(awardCommendations.ContainsKey(player1));
        Assert.IsTrue(awardCommendations.ContainsKey(player2));
        Assert.AreEqual(commendation0, awardCommendations[player0].commendation);
        Assert.AreEqual(defaultCommendation, awardCommendations[player1].commendation);
        Assert.AreEqual(defaultCommendation, awardCommendations[player2].commendation);
        Assert.AreEqual(0, awardCommendations[player0].place);
        Assert.AreEqual(1, awardCommendations[player1].place);
        Assert.AreEqual(2, awardCommendations[player2].place);
    }
}