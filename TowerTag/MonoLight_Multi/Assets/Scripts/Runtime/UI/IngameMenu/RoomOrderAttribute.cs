using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class RoomOrderAttribute : MonoBehaviour {
        [SerializeField] private RoomSorter.OrderAttribute _attribute;
        [SerializeField] private RoomSorter _roomSorter;
        [SerializeField] private Image _orderDirection;

        public RoomSorter.OrderAttribute Attribute => _attribute;

        private void Awake() {
            _orderDirection.enabled = false;
        }

        public void SetAttribute() {
            _roomSorter.SetOrderType(this);
        }

        public void SetOrderDirectionImage(bool ascending) {
            if (_orderDirection != null)
                _orderDirection.fillOrigin = ascending ? (int) Image.OriginVertical.Top : (int) Image.OriginVertical.Bottom;
            else
                Debug.LogWarning("Can't display order direction, reference to image missing");
        }

        public void ToggleOrderImage(bool setActive) {
            if (_orderDirection != null)
                _orderDirection.enabled = setActive;
            else
                Debug.LogWarning("Can't toggle order direction, reference to image missing");
        }
    }
}