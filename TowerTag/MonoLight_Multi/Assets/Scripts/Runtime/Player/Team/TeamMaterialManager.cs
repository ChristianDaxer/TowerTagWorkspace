using System;
using SOEventSystem.Shared;
using UnityEngine;

namespace TowerTag {
    [CreateAssetMenu(menuName = "TowerTag/Team Material Manager", fileName = "TeamMaterialManager")]
    public class TeamMaterialManager : ScriptableObjectSingleton<TeamMaterialManager> {
        [SerializeField] private TeamManager _teamManager;

        [Header("UI Materials")] [SerializeField]
        private Material _holoFire;

        [SerializeField] private Material _holoIce;
        [SerializeField] private Material _holoDefault;

        [SerializeField] private Material _holoAnimatedDefault;
        [SerializeField] private Material _holoAnimatedHighlight;

        [SerializeField] private Material _holoSeeThroughIce;
        [SerializeField] private Material _holoSeeThroughFire;

        [SerializeField] private Material _holoTwoFacedIce;
        [SerializeField] private Material _holoTwoFacedFire;

        [SerializeField] private Material _holoIceMedDark;

        [SerializeField] private Material _holoIceDark;
        [SerializeField] private Material _holoFireDark;
        [SerializeField] private Material _holoDefaultDark;

        [SerializeField] private Material _flatIce;
        [SerializeField] private Material _flatFire;
        [SerializeField] private Material _flatDefault;

        [SerializeField] private Material _flatIceMedDark;

        [SerializeField] private Material _flatIceDark;
        [SerializeField] private Material _flatFireDark;
        [SerializeField] private Material _flatDefaultDark;

        [SerializeField] private Material _flatDisabled;

        [SerializeField] private Material _dividerHighlightNeutral;
        [SerializeField] private Material _dividerHighlightIce;
        [SerializeField] private Material _dividerHighlightFire;

        [SerializeField] private Material _holoPopUpIce;
        [SerializeField] private Material _holoPopUpFire;
        [SerializeField] private Material _holoPopUpDisabledIce;
        [SerializeField] private Material _holoPopUpDisabledFire;
        [SerializeField] private Material _holoPopUpTextIce;
        [SerializeField] private Material _holoPopUpTextFire;
        [SerializeField] private Material _holoPopUpTextDisabledIce;
        [SerializeField] private Material _holoPopUpTextDisabledFire;

        [Header("Settings")] [SerializeField] private TeamID _uiDefaultTeam = TeamID.Ice;
        [SerializeField] private TeamID _uiHighlightTeam = TeamID.Fire;

        private static readonly int _color = Shader.PropertyToID("_Color");
        private static readonly int _fresnelColor = Shader.PropertyToID("_FresnelColor");
        private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");

        private void OnValidate() {
            Init();
        }

        public void Init() {
            _holoIce.SetColor(_color, _teamManager.TeamIce.Colors.UI);
            _holoIce.SetColor(_fresnelColor, _teamManager.TeamIce.Colors.UI);
            _holoFire.SetColor(_color, _teamManager.TeamFire.Colors.UI);
            _holoFire.SetColor(_fresnelColor, _teamManager.TeamFire.Colors.UI);
            _holoDefault.SetColor(_color, _teamManager.Get(_uiDefaultTeam).Colors.UI);
            _holoDefault.SetColor(_fresnelColor, _teamManager.Get(_uiDefaultTeam).Colors.UI);

            _holoAnimatedDefault.SetColor(_color, _teamManager.Get(_uiDefaultTeam).Colors.UI);
            _holoAnimatedDefault.SetColor(_fresnelColor, _teamManager.Get(_uiDefaultTeam).Colors.UI);
            _holoAnimatedHighlight.SetColor(_color, _teamManager.Get(_uiHighlightTeam).Colors.UI);
            _holoAnimatedHighlight.SetColor(_fresnelColor, _teamManager.Get(_uiHighlightTeam).Colors.UI);

            _holoSeeThroughIce.SetColor(_color, _teamManager.Get(_uiDefaultTeam).Colors.UI);
            _holoSeeThroughIce.SetColor(_fresnelColor, _teamManager.Get(_uiDefaultTeam).Colors.UI);
            _holoSeeThroughFire.SetColor(_color, _teamManager.Get(_uiHighlightTeam).Colors.UI);
            _holoSeeThroughFire.SetColor(_fresnelColor, _teamManager.Get(_uiHighlightTeam).Colors.UI);

            _holoTwoFacedIce.SetColor(_color, _teamManager.TeamIce.Colors.UI);
            _holoTwoFacedIce.SetColor(_fresnelColor, _teamManager.TeamIce.Colors.UI);
            _holoTwoFacedFire.SetColor(_color, _teamManager.TeamFire.Colors.UI);
            _holoTwoFacedFire.SetColor(_fresnelColor, _teamManager.TeamFire.Colors.UI);

            _holoIceMedDark.SetColor(_color, _teamManager.TeamIce.Colors.MediumDark);
            _holoIceMedDark.SetColor(_fresnelColor, _teamManager.TeamIce.Colors.MediumDark);
            _holoIceDark.SetColor(_color, _teamManager.TeamIce.Colors.DarkUI);
            _holoIceDark.SetColor(_fresnelColor, _teamManager.TeamIce.Colors.DarkUI);
            _holoFireDark.SetColor(_color, _teamManager.TeamFire.Colors.DarkUI);
            _holoFireDark.SetColor(_fresnelColor, _teamManager.TeamFire.Colors.DarkUI);
            _holoDefaultDark.SetColor(_color, _teamManager.Get(_uiDefaultTeam).Colors.DarkUI);
            _holoDefaultDark.SetColor(_fresnelColor, _teamManager.Get(_uiDefaultTeam).Colors.DarkUI);

            _flatIce.SetColor(_color, _teamManager.TeamIce.Colors.UI);
            _flatFire.SetColor(_color, _teamManager.TeamFire.Colors.UI);
            _flatDefault.SetColor(_color, _teamManager.Get(_uiDefaultTeam).Colors.UI);

            _flatIceMedDark.SetColor(_color, _teamManager.TeamIce.Colors.MediumDark);
            _flatIceDark.SetColor(_color, _teamManager.TeamIce.Colors.DarkUI);
            _flatFireDark.SetColor(_color, _teamManager.TeamFire.Colors.DarkUI);
            _flatDefaultDark.SetColor(_color, _teamManager.Get(_uiDefaultTeam).Colors.DarkUI);

            _dividerHighlightIce.SetColor(_emissionColor, _teamManager.TeamIce.Colors.Dark);
            _dividerHighlightNeutral.SetColor(_emissionColor, _teamManager.TeamNeutral.Colors.Dark);
            _dividerHighlightFire.SetColor(_emissionColor, _teamManager.TeamFire.Colors.Dark);
            
            _holoPopUpIce.SetColor(_color, _teamManager.TeamIce.Colors.UI);
            _holoPopUpFire.SetColor(_color, _teamManager.TeamFire.Colors.UI);
            _holoPopUpDisabledIce.SetColor(_color, _teamManager.TeamIce.Colors.DarkUI);
            _holoPopUpDisabledFire.SetColor(_color, _teamManager.TeamFire.Colors.DarkUI);
            _holoPopUpTextIce.SetColor(_color, _teamManager.TeamIce.Colors.UI);
            _holoPopUpTextIce.SetColor(_fresnelColor, _teamManager.TeamIce.Colors.UI);
            _holoPopUpTextFire.SetColor(_color, _teamManager.TeamFire.Colors.UI);
            _holoPopUpTextFire.SetColor(_fresnelColor, _teamManager.TeamFire.Colors.UI);
            _holoPopUpTextDisabledIce.SetColor(_color, _teamManager.TeamIce.Colors.DarkUI);
            _holoPopUpTextDisabledIce.SetColor(_fresnelColor, _teamManager.TeamIce.Colors.DarkUI);
            _holoPopUpTextDisabledFire.SetColor(_color, _teamManager.TeamFire.Colors.DarkUI);
            _holoPopUpTextDisabledFire.SetColor(_fresnelColor, _teamManager.TeamFire.Colors.DarkUI);
        }

        public Material GetHoloUI(TeamID teamID) {
            switch (teamID) {
                case TeamID.Ice:
                    return _holoIce;
                case TeamID.Fire:
                    return _holoFire;
                case TeamID.Neutral:
                    return _holoDefault;
                default:
                    throw new ArgumentOutOfRangeException(nameof(teamID), teamID, null);
            }
        }

        public Material GetFlatUI(TeamID teamID) {
            switch (teamID) {
                case TeamID.Ice:
                    return _flatIce;
                case TeamID.Fire:
                    return _flatFire;
                case TeamID.Neutral:
                    return _flatDefault;
                default:
                    throw new ArgumentOutOfRangeException(nameof(teamID), teamID, null);
            }
        }

        public Material GetFlatUIDark(TeamID teamID) {
            switch (teamID) {
                case TeamID.Ice:
                    return _flatIceDark;
                case TeamID.Fire:
                    return _flatFireDark;
                case TeamID.Neutral:
                    return _flatDefaultDark;
                default:
                    throw new ArgumentOutOfRangeException(nameof(teamID), teamID, null);
            }
        }

        public Material GetFlatUIDisabled() {
            return _flatDisabled;
        }
    }
}