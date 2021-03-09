/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Holodeck
{
    public class StartObserver : MonoBehaviour
    {
        [Header("--- CONNECTION SETTINGS ---")]
        public ConnectionSettings connectionMode = ConnectionSettings.DEFAULT;
        [Tooltip("Standard IP is 192.168.1.10, the value of this field is only used in 'Connection Custom Mode'")]
        public string holodeckIP = "192.168.1.10";
        [Tooltip("Standard port is 80, the value of this field is only used in 'Connection Custom Mode'")]
        public int holodeckPort = 80;
        public int[] IDs;

        [SerializeField]
        public string[] namesString;

        /// <summary>
        /// Starting Holodeck in Observer Mode, no player is here
        /// </summary>
        void Start()
        {
            GeneralAPI.ObserverMode = true;
            if(connectionMode == ConnectionSettings.CUSTOM)
            {
                GeneralAPI.SetHolodeckIP(holodeckIP);
                GeneralAPI.SetHolodeckPort(holodeckPort);
            }
            GeneralAPI.Start();
        }

        /// <summary>
        /// Gets all IDs from IDApi
        /// </summary>
        private void Update()
        {
            if (!GeneralAPI.IsHolodeckReady()) return;
            IDs = IdAPI.GetIds().ToArray();
            WriteVal();
    }

        /// <summary>
        /// Stopping the Holodeck
        /// </summary>
        private void OnDestroy()
        {
            GeneralAPI.Stop();
        }


        private void WriteVal()
        {

            var names = GeneralAPI.NameList;
            namesString = new string[names.Count];
            int j = 0;
            foreach(int i in names.Keys)
            {
                namesString[j++] = i + " " + names[i] + System.Environment.NewLine;
            }
        }
    }
}
