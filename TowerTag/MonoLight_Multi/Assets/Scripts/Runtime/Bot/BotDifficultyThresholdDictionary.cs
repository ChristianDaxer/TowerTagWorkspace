using System;
using AI;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct BotDifficultyThreshold
{
    [FormerlySerializedAs("Increase")] public float IncreaseThreshold;
    [FormerlySerializedAs("Decrease")] public float DecreaseThreshold;
}

/// <summary>
/// This is used for autoskilled bots.
/// The difficulty describes the difficulty the current bot has.
/// The integer describes how much of a score difference (in negative and positive direction)
/// is needed to increase to next higher or decrease to next lower difficulty.
/// </summary>
[Serializable]
public class BotDifficultyThresholdDictionary : SerializableDictionaryBase<BotBrain.BotDifficulty, BotDifficultyThreshold> {
}