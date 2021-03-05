using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using TowerTag;
using UnityEngine;

public static class AutoskillManager {
    // Average difficulty might be used at a later point
    private static BotBrain.BotDifficulty _averageDifficulty = BotBrain.BotDifficulty.Medium;

    public static BotBrain.BotDifficulty AverageDifficultyFromPreviousGames => _averageDifficulty;

    public static void SetAverageDifficulty(params BotBrain[] players) {
        var average = players.Sum(player => (float) player.Difficulty);

        _averageDifficulty = ((BotBrain.BotDifficulty) Mathf.RoundToInt(average));
    }
}