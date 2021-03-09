/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;
using Holodeck;

/// <summary>
/// Manages spawning and despawning of ingame objects 
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Standard enemies")]
    //the prefab to instantiate when a new id is received
    [Tooltip("If its empty, enemy representation is spawned in root directory. If you put in a transform here, this transform will be the parent for all spawned enemy representations.")]
    public Transform enemyRoot;
    [Tooltip("GameObject with ObjectPositioner Component as representation for other active tracked Holodeck-Tags. If empty, the standard enemy representation is spawned for every active Holodeck-Tag")]
    public ObjectPositioner standardEnemy;

    [Header("Standard special object")]
    [Tooltip("GameObject with ObjectPositioner Component as representation for other active tracked Holodeck-Tags with a special type. If empty, the standard typedObject representation is spawned for every active Holodeck-Tag")]
    public ObjectPositioner standardTypedObject;

    [Serializable]
    public struct SpecialObject
    {
        [Tooltip("This is the prefab which should be spawned if the object type is detected")]
        public GameObject objectPrefab;
        [Tooltip("This is the name of your typed object like it is named in the config server")]
        public string tagType;
    }

    [Header("User defined special objects")]
    [Tooltip("During development in Unity, define here which Prefab should be spawned for which typedID.\n" +
             "Which ID has a special type is defined at the holodeck config server."        )]
    public SpecialObject[] typedObjects;

    // Keeps track of the currently managed and tracked non-player entities
    public Dictionary<int, GameObject> enemyInstances = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> typedObjectInstances = new Dictionary<int, GameObject>();

    // Use this for initialization
    void Start()
    {
        //Check if custom enemy is set
        if (standardEnemy == null)
        {
            standardEnemy = Resources.Load<ObjectPositioner>("Enemy");
        }
        if (standardTypedObject == null)
        {
            standardTypedObject = standardEnemy;
        }

        GeneralAPI.HolodeckStateChanged += InitializeEnemies;
    }

    public List<int> ids;
    /// <summary>
    /// Initializing the enemies
    /// </summary>
    /// <param name="state"></param>
    public void InitializeEnemies(Holodeck.HolodeckState state)
    {
        if(state == HolodeckState.Ready)
        {
            ids = IdAPI.GetIds();
            for(int i = 0; i < ids.Count; i++)
            {
                if (IdAPI.HasType(ids[i]))
                {
                    SpawnTypedObject(ids[i]);
                }
                else
                {

                    SpawnEnemy(ids[i]);

                }
            }
            //Setup api events
            IdAPI.IDEntered += IDEntered;
            IdAPI.IDLeft += IDLeft;
            IdAPI.IDTypeAdded += IDTypeChanged;
            IdAPI.IDTypeRemoved += IDTypeChanged;
            IdAPI.IDTypeChanged += IDTypeChanged;
        }
        if (state == HolodeckState.None)
        {
            enemyInstances.Clear();
            typedObjectInstances.Clear();
            //Setup api events
            IdAPI.IDEntered -= IDEntered;
            IdAPI.IDLeft -= IDLeft;
            IdAPI.IDTypeAdded -= IDTypeChanged;
            IdAPI.IDTypeRemoved -= IDTypeChanged;
            IdAPI.IDTypeChanged -= IDTypeChanged;
        }
    }

    /// <summary>
    /// Handler for idEntered event
    /// </summary>
    /// <param name="id"></param>
    void IDEntered(int id)
    {
        Debug.Log("[HDVR] Enemy is Spawned " + id);
        //Check if special
        if (IdAPI.HasType(id))
        {
            SpawnTypedObject(id);
        }
        else
        {
            SpawnEnemy(id);
        }
    }

    /// <summary>
    /// Handler for idLeft event
    /// </summary>
    /// <param name="id"></param>
    void IDLeft(int id)
    {
        if (enemyInstances.ContainsKey(id))
        {
            Destroy(enemyInstances[id]);
            enemyInstances.Remove(id);
        }
        if (typedObjectInstances.ContainsKey(id))
        {
            Destroy(typedObjectInstances[id]);
            typedObjectInstances.Remove(id);
        }
    }

    /// <summary>
    /// Handler for ID Type Changed Event. Basically removes the old prefab and spawns a new one.
    /// </summary>
    /// <param name="id"></param>
    void IDTypeChanged(int id)
    {
        if (!typedObjectInstances.ContainsKey(id)&& enemyInstances.ContainsKey(id))
            {
            Destroy(enemyInstances[id]);
            enemyInstances.Remove(id);

            SpawnTypedObject(id);
        }
        if (typedObjectInstances.ContainsKey(id))
        {
            Destroy(typedObjectInstances[id]);
            typedObjectInstances.Remove(id);

            SpawnTypedObject(id);
        }
    }

    /// <summary>
    /// Creates a new typed object. If specified, spawns a certain prefab and adds an configured object positioner if necessary.
    /// Otherwise spawns a standard prefab.
    /// </summary>
    /// <param name="id"></param>
    void SpawnTypedObject(int id)
    {
        if (!typedObjectInstances.ContainsKey(id))
        {
            if (IdAPI.GetPlayerId() != id && id > 0)
            {
                GameObject typedObject;

                if (IdAPI.GetTypeofID(id) == null ||
                    IdAPI.GetTypeofID(id) == "" ||
                    GetPrefabOfType(IdAPI.GetTypeofID(id)) == null)
                {
                    typedObject = Instantiate<GameObject>(standardTypedObject.gameObject);
                }
                else
                {
                    String typeOfPrefab = IdAPI.GetTypeofID(id);
                    typedObject = Instantiate<GameObject>(GetPrefabOfType(typeOfPrefab));

                    if (typedObject.GetComponent<ObjectPositioner>() == null)
                    {
                        typedObject.AddComponent<ObjectPositioner>();
                    }
                }
                
                typedObject.GetComponent<ObjectPositioner>().id = id;
                typedObject.GetComponent<ObjectPositioner>().type = IdAPI.GetTypeofID(id);

                typedObjectInstances[id] = typedObject;
            }
        }
    }

    /// <summary>
    /// Creates a new StandardEnemy and configures it for the id.
    /// </summary>
    /// <param name="id"></param>
    void SpawnEnemy(int id)
    {
        if (!enemyInstances.ContainsKey(id))
        {
            if (IdAPI.GetPlayerId() != id && id > 0)
            {
                GameObject enemy = Instantiate<GameObject>(standardEnemy.gameObject);
                enemy.GetComponent<ObjectPositioner>().id = id;
                if (enemyRoot != null)
                    enemy.transform.parent = enemyRoot;
                enemyInstances[id] = enemy;
            }
        }
    }

    /// <summary>
    /// Returns the matching Prefab to a in unitys enemymanager defines type, or null if its not specified.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    GameObject GetPrefabOfType(String type)
    {
        for (int i = 0; i < typedObjects.Length; i++)
        {
            if (typedObjects[i].tagType == type)
            {
                return typedObjects[i].objectPrefab;
            }
        }
        return null;
    }
}