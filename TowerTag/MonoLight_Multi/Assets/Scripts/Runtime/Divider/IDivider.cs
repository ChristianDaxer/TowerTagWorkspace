namespace TowerTag.Divider {
    public interface IDivider {
        void ResetHighlight();
        void SetHighlight(float value);
        void SetHighlight(float value, TeamID teamID);
    }
}