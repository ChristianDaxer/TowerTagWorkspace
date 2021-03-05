using System;
using System.Globalization;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class StatisticLine : MonoBehaviour {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private Image _background;
        private RoomLine.BackgroundStyle _backgroundStyle;


        public void Init(PlayerStatistic stat, RoomLine.BackgroundStyle style) {
            SetDisplayText(stat.DisplayName);
            SetValue(stat.GetValueForLocalPlayer(), stat.Type);
            SetStyle(style);
        }

        public void SetDisplayText(string displayText) {
            _name.text = displayText;
        }

        public void SetValue(float value, PlayerStatistic.StatisticType statType) {
            if (statType == PlayerStatistic.StatisticType.Seconds)
            {
                TimeSpan time = TimeSpan.FromSeconds(value);
                string minutes = time.Minutes < 10 ? $"0{time.Minutes}" : $"{time.Minutes}";
                string hours = time.TotalHours < 10 ? $"0{(int)time.TotalHours}" : $"{(int)time.TotalHours}";
                _value.text = $"{hours}:{minutes}";
            }
            else {
                _value.text = value % 1 > 0
                    ? value.ToString("F2")
                    : value.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void SetStyle(RoomLine.BackgroundStyle style) {
            Color baseColor = TeamManager.Singleton.TeamIce.Colors.UI;
            if (style == RoomLine.BackgroundStyle.Dark) {
                _background.color = baseColor * 0.0f;
                _name.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _value.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }

            if (style == RoomLine.BackgroundStyle.Light) {
                _background.color = baseColor * 0.3f;
                _name.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _value.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }
        }
    }
}