using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool that replace existing Pillar Scene Objects with an Instance of serialized Pillar Prefab.
/// Install: Place this Script on the "Pillars" Parent MonoBehavior in your Scene, serialize the new Pillar Prefab und Check the Context Menu of the Script Component
/// for the Replace Function.
/// </summary>
/// <author>Sebastian Krebs (sebastian.krebs@vrnerds.de)</author>
public class PillarReplaceTool : MonoBehaviour {
    /// <summary>
    /// New Prefab which is used to instantiate the new Pillars Objects
    /// </summary>
    [SerializeField] private GameObject _pillarPrefab;


    /// <summary>
    /// Context Menu-Method to Start the replacing
    /// </summary>
    [ContextMenu("Replace Pillars in Scene")]
    private void FindAndReplacePillar() {
#if UNITY_EDITOR
        if (_pillarPrefab == null) {
            Debug.LogError("Check Serialized Field: no Pillar Prefab found.");
            return;
        }

        //find all Pillars in Scene
        Pillar[] pillars = FindObjectsOfType<Pillar>();
        var idName = 0;

        //replace all GameObjects with new Instance of Pillar Prefab
        foreach (Pillar oldPillar in pillars) {
            //Instantiate Object; Set Transform & Parent
            Transform parent = oldPillar.GetComponentInParent<SimpleLODGroup>().transform;
            var newPillar = PrefabUtility.InstantiatePrefab(_pillarPrefab) as GameObject;
            if (newPillar == null)
                continue;
            newPillar.name = newPillar.name + idName;
            idName++;


            newPillar.transform.SetPositionAndRotation(parent.position, parent.rotation);
            newPillar.transform.SetParent(FindObjectOfType<PillarReplaceTool>().transform);

            //Replace Walls
            try {
                var oldWalls = parent.GetComponentInChildren<PillarWalls>().Walls;
                var newWalls = newPillar.GetComponentInChildren<PillarWalls>().Walls;
                ReplacePillarWalls(oldWalls, newWalls);
            }
            catch (Exception e) {
                Debug.LogWarning("Found Pillar (maybe Spectator Pillar) without Walls " + e);
                newPillar.GetComponentInChildren<PillarWalls>().transform.gameObject.SetActive(false);
            }

            //Set Pillar Settings
            CheckPillarSetting(newPillar.GetComponent<Pillar>(), oldPillar);

            //Check Spawn/Spectator/Goal Pillar and Tag it
            if (newPillar.GetComponent<Pillar>().IsSpawnPillar)
                newPillar.name = newPillar.name + "Spawn";
            if (newPillar.GetComponent<Pillar>().IsGoalPillar)
                newPillar.name = newPillar.name + "Goal";
            if (newPillar.GetComponent<Pillar>().IsSpectatorPillar)
                newPillar.name = newPillar.name + "Spectator";
        }

        Debug.Log("Replaced " + pillars.Length + " Pillar");

        //Delete old Pillars
        foreach (Pillar oldPillar in pillars) {
            DestroyImmediate(oldPillar.GetComponentInParent<SimpleLODGroup>().transform.gameObject);
        }
#endif
    }

    private static void ReplacePillarWalls(PillarWall[] oldWalls, PillarWall[] newWalls) {
        //Deactivate redundant Walls
        if (oldWalls.Length < newWalls.Length) {
            for (int i = oldWalls.Length; i <= newWalls.Length; i++) {
                newWalls[i].transform.gameObject.SetActive(false);
            }
        }

        //Replace Walls
        for (var i = 0; i < oldWalls.Length; i++) {
            newWalls[i].transform
                .SetPositionAndRotation(oldWalls[i].transform.position, oldWalls[i].transform.rotation);
        }
    }

    private static void CheckPillarSetting(Pillar dst, Pillar src) {
        dst.IsSpawnPillar = src.IsSpawnPillar;
        dst.IsGoalPillar = src.IsGoalPillar;
        dst.ShowOrientationHelp = src.ShowOrientationHelp;
        dst.IsClaimable = src.IsClaimable;
        dst.OwningTeamID = src.OwningTeamID;
        dst.IsSpectatorPillar = src.IsSpectatorPillar;
        dst.IsClaimable = src.IsClaimable;
    }
}