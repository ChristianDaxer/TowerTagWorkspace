using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Home.UI {
    public class RoomSorter : MonoBehaviour {
        [Serializable]
        public enum OrderAttribute {
            RoomName,
            GameMode,
            Map,
            Status,
            Player,
            Rank,
            Ping,
            Undefined
        }

        private RoomOrderAttribute _currentOrderAttribute;

        private bool _ascend = true;

        public void SetOrderType(RoomOrderAttribute attributeType) {
            if (_currentOrderAttribute == attributeType) {
                _ascend = !_ascend;
            }
            else {
                if(_currentOrderAttribute != null) _currentOrderAttribute.ToggleOrderImage(false);
                attributeType.ToggleOrderImage(true);
                _currentOrderAttribute = attributeType;
            }
            _currentOrderAttribute.SetOrderDirectionImage(_ascend);
        }

        public Dictionary<string, RoomLine> SortRoomsByCurrentOrder(Dictionary<string, RoomLine> roomInfoList) {
            if (_currentOrderAttribute == null) return roomInfoList;
            switch (_currentOrderAttribute.Attribute) {
                case OrderAttribute.RoomName:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.RoomName)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.RoomName)
                            .ToDictionary(p => p.Key, p => p.Value);
                case OrderAttribute.GameMode:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.GameMode)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.GameMode)
                            .ToDictionary(p => p.Key, p => p.Value);
                case OrderAttribute.Map:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.Map)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.Map)
                            .ToDictionary(p => p.Key, p => p.Value);
                case OrderAttribute.Status:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.RoomState)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.RoomState)
                            .ToDictionary(p => p.Key, p => p.Value);
                case OrderAttribute.Player:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.MaxPlayers)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.MaxPlayers)
                            .ToDictionary(p => p.Key, p => p.Value);
                case OrderAttribute.Rank:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.MinRank)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.MinRank)
                            .ToDictionary(p => p.Key, p => p.Value);
                case OrderAttribute.Ping:
                    return _ascend
                        ? roomInfoList.OrderBy(element => element.Value.Data.Ping)
                            .ToDictionary(p => p.Key, p => p.Value)
                        : roomInfoList.OrderByDescending(element => element.Value.Data.Ping)
                            .ToDictionary(p => p.Key, p => p.Value);
                default: return roomInfoList;
            }
        }
    }
}