/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649

namespace Holodeck
{
    public class SpawnObjects : MonoBehaviour
    {
        #region VARIABLES_UNITY_INSPECTOR
        public bool onlyFloor;
        [Header("Spawnable Objects")]
        [SerializeField]
        private GameObject cylinder_small;
        [SerializeField]
        private GameObject cylinder_big;
        [SerializeField]
        private GameObject cube_small;
        [SerializeField]
        private GameObject cube_medium;
        [SerializeField]
        private GameObject cube_big_vertical;
        [SerializeField]
        private GameObject cube_big_horizontal;
        [SerializeField]
        private GameObject corner_left_top;
        [SerializeField]
        private GameObject corner_right_top;
        [SerializeField]
        private GameObject corner_left_bottom;
        [SerializeField]
        private GameObject corner_right_bottom;
        #endregion

        #region VARIABLES PRIVATE
        private Transform parent;
        private Bounds bounds;
        private float playfieldSquare;
        private List<GameObject> allGameobjects = new List<GameObject>();

        private List<Vector3> positionList = new List<Vector3>();

        private bool corner_left_t = false;
        private bool corner_left_b = false;
        private bool corner_right_t = false;
        private bool corner_right_b = false;
        #endregion

        #region INITIALIZE
        /// <summary>
        /// Main method to create the objects on the playfield
        /// </summary>
        /// <param name="_bounds"></param>
        /// <param name="_parent"></param>
        public void CreateObjects(Bounds _bounds, Transform _parent)
        {
            parent = _parent;
            bounds = _bounds;
            playfieldSquare = bounds.size.x * bounds.size.z;
            if (onlyFloor) return;
            CreateCorners();
            CreateSmallObjects(cube_small);
            CreateMediumObjects(cube_medium);
            CreateSmallObjects(cylinder_small);
            CreateMediumObjects(cylinder_big);
            CreateBigCubes();
        }
        #endregion

        #region CREATE_OBJECTS
        /// <summary>
        /// Creating the corner game objects
        /// </summary>
        private void CreateCorners()
        {
            //Corner Vectors
            Vector3 leftBottom = new Vector3(0.5f, 0.0f, 0.5f);
            Vector3 rightBottom = new Vector3(bounds.size.x - 0.5f, 0.0f, 0.5f);
            Vector3 leftTop = new Vector3(0.5f, 0.0f, bounds.size.z - 0.5f);
            Vector3 rightTop = new Vector3(bounds.size.x - 0.5f, 0.0f, bounds.size.z - 0.5f);

            GameObject go;

            int _rand = Random.Range(1, 5);

            for (int i = 0; i <= _rand; i++)
            {
                switch (i)
                {
                    case 0:
                        go = GameObject.Instantiate(corner_left_bottom);
                        go.transform.position = leftBottom;
                        go.transform.parent = parent;
                        corner_left_b = true;
                        allGameobjects.Add(go);
                        break;
                    case 1:
                        go = GameObject.Instantiate(corner_right_top);
                        go.transform.position = rightTop;
                        go.transform.parent = parent;
                        corner_right_t = true;
                        allGameobjects.Add(go);
                        break;
                    case 2:
                        go = GameObject.Instantiate(corner_left_top);
                        go.transform.position = leftTop;
                        go.transform.parent = parent;
                        corner_left_t = true;
                        allGameobjects.Add(go);
                        break;
                    case 3:
                        go = GameObject.Instantiate(corner_right_bottom);
                        go.transform.position = rightBottom;
                        go.transform.parent = parent;
                        corner_right_b = true;
                        allGameobjects.Add(go);
                        break;
                }
            }
        }

        /// <summary>
        /// Creating a few small sized objects
        /// </summary>
        /// <param name="_prefab"></param>
        private void CreateSmallObjects(GameObject _prefab)
        {
            if (playfieldSquare < 100.0f) return;
            float _maxRange = playfieldSquare / 33;
            int _rand = Random.Range(1, (int)_maxRange);

            GameObject go;

            for (int i = 0; i < _rand; i++)
            {
                go = Instantiate(_prefab);
                go.transform.parent = parent;
                go.transform.localPosition = CreateCubePosition(0.5f, 1.0f, 1.0f);
                allGameobjects.Add(go);
            }
        }

        /// <summary>
        /// Creating a few medium sized objects
        /// </summary>
        /// <param name="_prefab"></param>
        private void CreateMediumObjects(GameObject _prefab)
        {
            if (playfieldSquare < 100.0f) return;
            float _maxRange = playfieldSquare / 44;
            int _rand = Random.Range(1, (int)_maxRange);

            GameObject go;

            for (int i = 0; i < _rand; i++)
            {
                go = Instantiate(_prefab);
                go.transform.parent = parent;
                go.transform.localPosition = CreateCubePosition(1.0f, 1.0f, 1.0f);
                allGameobjects.Add(go);
            }
        }

        /// <summary>
        /// Creating a few big sized cubes
        /// </summary>
        private void CreateBigCubes()
        {
            if (playfieldSquare < 100.0f) return;
            float _maxRange = playfieldSquare / 55;
            int _rand = Random.Range(1, (int)_maxRange);

            GameObject go;

            for (int i = 0; i < _rand; i++)
            {
                bool _allowInstance = false;
                int _whichOneRand = Random.Range(0, 4);
                if (_whichOneRand == 0 || _whichOneRand == 2)
                {
                    go = Instantiate(cube_big_horizontal);
                    allGameobjects.Add(go);
                    if (!corner_left_b && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(3.0f, 1.0f, 0.5f);
                        corner_left_b = true;
                        _allowInstance = true;
                    }
                    else if (!corner_left_t && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(3.0f, 1.0f, bounds.size.z - 0.5f);
                        corner_left_t = true;
                        _allowInstance = true;
                    }
                    else if (!corner_right_b && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(bounds.size.x - 3.0f, 1.0f, 0.5f);
                        corner_right_b = true;
                        _allowInstance = true;
                    }
                    else if (!corner_right_t && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(bounds.size.x - 3.0f, 1.0f, bounds.size.z - 0.5f);
                        corner_right_t = true;
                        _allowInstance = true;
                    }
                    if (!_allowInstance) Destroy(go);
                }
                else
                {
                    go = Instantiate(cube_big_vertical);
                    allGameobjects.Add(go);
                    if (!corner_left_b && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(0.5f, 1.0f, 3.0f);
                        corner_left_b = true;
                        _allowInstance = true;
                    }
                    else if (!corner_left_t && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(bounds.size.x - 0.5f, 1.0f, 3.0f);
                        corner_left_t = true;
                        _allowInstance = true;
                    }
                    else if (!corner_right_b && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(0.5f, 1.0f, bounds.size.z - 3.0f);
                        corner_right_b = true;
                        _allowInstance = true;
                    }
                    else if (!corner_right_t && !_allowInstance)
                    {
                        go.transform.localPosition = new Vector3(bounds.size.x - 0.5f, 1.0f, bounds.size.z - 3.0f);
                        corner_right_t = true;
                        _allowInstance = true;
                    }
                    if (!_allowInstance) Destroy(go);
                }
                go.transform.parent = parent;
            }
        }
        #endregion

        #region HELPERS
        /// <summary>
        /// Returning the positions for the cubes
        /// </summary>
        /// <param name="_height"></param>
        /// <param name="_offsetX"></param>
        /// <param name="_offsetY"></param>
        /// <returns></returns>
        private Vector3 CreateCubePosition(float _height, float _offsetX, float _offsetY)
        {
            Vector3 _addVal;
            Vector3 _retVal;
            do
            {
                float _x = 2.0f;
                float _y = 2.0f;

                int _randX = Random.Range(1, (int)(bounds.size.x - 2 * _x));
                int _randY = Random.Range(1, (int)(bounds.size.z - 2 * _y));

                _addVal = new Vector3(_x + _randX, 0.0f, _y + _randY);
                _retVal = new Vector3(_x + _randX, _height, _y + _randY);

                if (!positionList.Contains(_addVal))
                {
                    positionList.Add(_addVal);
                }

            } while (!positionList.Contains(_addVal));

            return _retVal;
        }
        #endregion

        #region RESET_THE_SCENE
        /// <summary>
        /// Reset all game objects
        /// </summary>
        public void ResetScene()
        {
            corner_left_t = false;
            corner_left_b = false;
            corner_right_t = false;
            corner_right_b = false;

            DeleteGameObjects();

            allGameobjects = new List<GameObject>();
            positionList = new List<Vector3>();
        }

        /// <summary>
        /// Destroying all GameObjects
        /// </summary>
        private void DeleteGameObjects()
        {
            foreach (GameObject g in allGameobjects)
            {
                Destroy(g);
            }
        }
        #endregion
    }
}
