using JetBrains.Annotations;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using TowerTag;

public class ImageColorizer : MonoBehaviour {
    [SerializeField] private Image[] _imagesToTint;
    [SerializeField] private TMP_Text[] _textsToTint;

    [UsedImplicitly]
    public void ColorizeImages(TeamID teamID) {
        if(_imagesToTint.Length > 0)
            _imagesToTint.ForEach(image => image.material = TeamMaterialManager.Singleton.GetFlatUI(teamID));
        if (_textsToTint.Length > 0)
            _textsToTint.ForEach(text => text.color = TeamManager.Singleton.Get(teamID).Colors.UI);
    }
}
