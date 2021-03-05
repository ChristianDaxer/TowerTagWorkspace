using UnityEngine;
using VRNerdsUtilities;

public class BadgeManager : SingletonMonoBehaviour<BadgeManager> {
    [SerializeField] private Sprite[] _badges;

    public Sprite GetBadgeByLevel(int level) {
        if (level <= 0)
            return _badges[0];

        return level >= _badges.Length ? _badges[_badges.Length - 1] : _badges[level - 1];
    }
}