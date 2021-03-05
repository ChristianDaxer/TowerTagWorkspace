using SOEventSystem.Shared;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchDescriptionCollection", menuName = "TowerTag/MatchDescriptionCollection")]
public class MatchDescriptionCollection : ScriptableObjectSingleton<MatchDescriptionCollection>
{
    // Collection of  Match descriptions, editable in Inspector
    [SerializeField, Tooltip("Collection of Match descriptions, use this to group allowed Matches.")]
    public MatchDescription[] _matchDescriptions;

    public void OnValidate()
    {
        var valid = true;
        var tmp = new List<int>();
        _matchDescriptions = _matchDescriptions.Where(desc => desc != null).ToArray();
        foreach (MatchDescription match in _matchDescriptions)
        {
            if (tmp.Contains(match.MatchID) || match.MatchID >= _matchDescriptions.Length)
            {
                valid = false;
                break;
            }

            tmp.Add(match.MatchID);
        }

        if (!valid)
        {
            for (var i = 0; i < _matchDescriptions.Length; i++)
            {
                _matchDescriptions[i].MatchID = i;
            }
        }
    }

    /// <summary>
    /// Get a MatchDescription by index (Index of the description in the matchDescriptions array).
    /// </summary>
    /// <param name="matchDescriptionIndex">Index of the description in the matchDescriptions array. To iterate over all match descriptions use this in combination with GetMatchDescriptionCount.</param>
    /// <returns>The MatchDescription corresponding to the matchDescriptions array index.</returns>
    public MatchDescription GetMatchDescription(int matchDescriptionIndex)
    {
        if (_matchDescriptions == null)
        {
            Debug.LogError(
                "MatchDescriptionCollection.GetMatchDescription: can't get matchDescription -> matchDescriptions array is null!");
            return null;
        }

        if (matchDescriptionIndex >= 0 && matchDescriptionIndex < _matchDescriptions.Length)
        {
            MatchDescription matchDesc = _matchDescriptions[matchDescriptionIndex];
            if (matchDesc != null)
                matchDesc.MatchID = matchDescriptionIndex;

            return _matchDescriptions[matchDescriptionIndex];
        }

        Debug.LogError("MatchDescriptionCollection.GetMatchDescription: can't get matchDescription, given index(" +
                       matchDescriptionIndex + ") is not valid!");
        return null;
    }

    public MatchDescription GetMatchDescription(GameMode gameMode, string map)
    {
        if (_matchDescriptions == null)
        {
            Debug.LogError(
                "MatchDescriptionCollection.GetMatchDescription: can't get matchDescription -> matchDescriptions array is null!");
            return null;
        }

        return _matchDescriptions.FirstOrDefault(desc =>
            desc.MapName.Equals(map) && desc.GameMode.HasFlag(gameMode));
    }
}