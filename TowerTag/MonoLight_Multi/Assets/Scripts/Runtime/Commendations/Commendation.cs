using TowerTag;
using UnityEngine;

namespace Commendations {
    public interface ICommendation {
        string name { get; }
        string DisplayName { get; }
        string Description { get; }
        Sprite Icon { get; }
        int Value { get; }
    }

    /// <summary>
    /// A commendation is awarded to a player after a match has finished.
    /// 
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public abstract class Commendation : ScriptableObject, ICommendation {
        [SerializeField, Tooltip("The displayed name of the commendation, e.g. \"M.V.P.\"")]
        private string _displayName;

        [SerializeField, Tooltip("A short description, e.g. \"Most Valuable Player\"")]
        private string _description;

        [SerializeField, Tooltip("This representative icon of the commendations.")]
        private Sprite _icon;

        [SerializeField, Range(0, 5), Tooltip("Determines how valuable this commendation is.")]
        private int _value;

        [SerializeField, Tooltip("List with Disabled GameModes")]
        private GameMode[] _disabledGameModes;

        [SerializeField, Tooltip("Choose the Team if Commendation is Team Based (set to neutral if not)")]
        private TeamID _teamID = TeamID.Neutral;

        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public int Value => _value;
        protected GameMode[] DisabledGameModes => _disabledGameModes;
        protected TeamID TeamID => _teamID;
    }
}