using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Hologate {
    public class HologateRoomSetup : MonoBehaviourPunCallbacks {
        public ApplyPillarOffset PillarOffset;

        public class DeviceOffsetDictionary : Dictionary<int, Vector3> {
            public new void Add(int deviceID, Vector3 offset) {
                base.Add(deviceID, offset);
            }

            public new Vector3 this[int deviceID] {
                get => base[deviceID];
                set => base[deviceID] = value;
            }
        }

        private readonly Dictionary<int, int> _deviceToRotation = new Dictionary<int, int> {
            {1, 0 },
            {2, 270 },
            {3, 90 },
            {4, 180 }
        };

        private static readonly DeviceOffsetDictionary _4PlayerSetup = new DeviceOffsetDictionary{
            {1, new Vector3(1.25f, 0, -1.25f)},
            {2, new Vector3(1.25f, 0,-1.25f)},
            {3, new Vector3(1.25f, 0, -1.25f)},
            {4, new Vector3(1.25f, 0, -1.25f)}
        };

        private static readonly DeviceOffsetDictionary _2PlayerSetup = new DeviceOffsetDictionary {
            {1, new Vector3(1.25f, 0, 0)},
            {2, new Vector3(-1.25f, 0, 0)},
            {3, new Vector3(1.25f, 0, 0)},
            {4, new Vector3(-1.25f, 0, 0)}
        };

        private static readonly DeviceOffsetDictionary _1PlayerSetup = new DeviceOffsetDictionary {
            {1, new Vector3(0, 0, 0)},
            {2, new Vector3(0, 0, 0)},
            {3, new Vector3(0, 0, 0)},
            {4, new Vector3(0, 0, 0)}
        };

        private readonly Dictionary<int, DeviceOffsetDictionary> _playerCountToOffset = new Dictionary<int, DeviceOffsetDictionary> {
            {1, _1PlayerSetup},
            {2, _2PlayerSetup},
            {4, _4PlayerSetup}
        };

        public void SetPlaySpaceOffset() {
            if (PhotonNetwork.CurrentRoom == null ||
                !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("PC"))
                return;
            int hgSetup = (int)PhotonNetwork.CurrentRoom.CustomProperties["PC"];
            int deviceID = HologateManager.MachineData.Data.ID;
            if (_playerCountToOffset.ContainsKey(hgSetup)) {
                ConfigurationManager.Configuration.PillarPositionOffset = _playerCountToOffset[hgSetup][deviceID];
                ConfigurationManager.WriteConfigToFile();
                PillarOffset.ApplyOffsetFromConfigurationFile();
                gameObject.transform.Rotate(Vector3.up, _deviceToRotation[deviceID]);
            } else {
                Debug.LogError("Could not apply pillar offset! Player count is not valid");
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
            if (propertiesThatChanged.ContainsKey("PC"))
                SetPlaySpaceOffset();
        }
    }
}