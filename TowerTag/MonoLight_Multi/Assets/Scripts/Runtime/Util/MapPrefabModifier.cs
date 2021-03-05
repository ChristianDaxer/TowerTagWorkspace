using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPrefabModifier : MonoBehaviour {
    [SerializeField] private GameObject _prefabToModify;
    [SerializeField] private Material _sockelMaterialNonMetallic;
    [SerializeField] private Material _wallMaterialStandard;
    [SerializeField] private Material _pillarLightStandard;
    [SerializeField] private string _sockelGameObjectName = "Sockel";
    [SerializeField] private string _pillarLightsGameObjectName = "PillarLights";
    [SerializeField] private string _sockelAndWallTag = "Dec_Metal";
    [SerializeField] private string _pillarLightsTag = "Dec_Glass";
    [SerializeField] private bool _addLightMapAsset;
    [SerializeField] private bool _switchMaterials;
    private List<MonoBehaviour> _behavioursToDestroy;


    public void ModifyPrefab() {
        if (_addLightMapAsset)
            _prefabToModify.AddComponent<PrefabLightmapData>();

        if (_switchMaterials) {
            GameObject[] sockels = GameObject.FindGameObjectsWithTag(_sockelAndWallTag)
                .Where(go => go.name.Equals(_sockelGameObjectName)).ToArray();
            GameObject[] pillarLights = GameObject.FindGameObjectsWithTag(_pillarLightsTag)
                .Where(go => go.name.Contains(_pillarLightsGameObjectName)).ToArray();
            PillarWall[] walls = FindObjectsOfType<PillarWall>();

            if (sockels.Length <= 0) {
                Debug.LogWarning($"No GameObjects with the Tag \"{_sockelAndWallTag}\" and name \"{_sockelGameObjectName}\" aborting material exchange");
                return;
            }

            sockels.ForEach(sockel => {
                if(sockel.GetComponent<Renderer>() != null)
                    sockel.GetComponent<Renderer>().material = _sockelMaterialNonMetallic;
            });
            walls.ForEach(wall => {
                if (wall.GetComponent<Renderer>() != null)
                    wall.gameObject.GetComponent<Renderer>().material = _wallMaterialStandard;
            });
            pillarLights.ForEach(pillarLight => {
                if (pillarLight.GetComponent<Renderer>() != null)
                    pillarLight.gameObject.GetComponent<Renderer>().material = _pillarLightStandard;
            });
        }
    }

}
