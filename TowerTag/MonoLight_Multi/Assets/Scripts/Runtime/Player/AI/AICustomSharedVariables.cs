using BehaviorDesigner.Runtime;


namespace AI {
    /// <summary>
    /// Custom Shared Variables for the AI Bot.
    /// 'SharedBotBrain' is used to reference and access properties of a BotBrain
    /// </summary>
    [System.Serializable]
    public class SharedBotBrain : SharedVariable<BotBrain>
    {
        public static implicit operator SharedBotBrain(BotBrain value) { return new SharedBotBrain { Value = value }; }
    }
}

