using System;
using System.Collections.Generic;
using System.Linq;
using RotaryHeart.Lib.SerializableDictionary;
using TowerTagSOES;
using UnityEngine;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Serializable]
        public struct GameObjectArray
        {
            public List<GameObject> GameObjects;
        }

        [Serializable]
        public class ControllerTypeUI : SerializableDictionaryBase<ControllerType, GameObjectArray>
        {
        }

        [SerializeField] private ControllerTypeUI _controllerTypeUI;

        private void Awake()
        {
            /*
            _controllerTypeUI.ForEach(kvP => kvP.Value.GameObjects.ForEach(go => go.SetActive(false)));
            _controllerTypeUI.FirstOrDefault(type => type.Key == SharedControllerType.Singleton).Value.GameObjects
                .ForEach(go => go.SetActive(true));
            */
        }
    }
}