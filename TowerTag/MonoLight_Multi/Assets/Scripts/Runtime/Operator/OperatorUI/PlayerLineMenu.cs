using System.Collections;
using System.Collections.Generic;
using PopUpMenu;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLineMenu : MonoBehaviour {
    [SerializeField, Tooltip("List of PlayerOptions")]
    private PlayerOption[] _options;

    [SerializeField, Tooltip("List of PlayerOptions which are excluded in BotPlayerLine")]
    private PlayerOption[] _botOptions;

    [SerializeField, Tooltip("Prefab of a button of the PlayerOptionMenu")]
    private GameObject _buttonPrefab;

    [SerializeField, Tooltip("The OptionList Child, the Panel for the Buttons")]
    private GameObject _optionList;

    //For the Lerp of the Frame (1 px top and bottom)
    private const float FrameThickness = 2;
    private const float LerpSpeed = 0.6f;

    private readonly List<GameObject> _buttons = new List<GameObject>();

    private RectTransform _rectTransform;
    private float _buttonHeight;

    private PlayerOption[] _menuOptions;
    private PlayerLineController _playerLineController;
    private IPlayer _player;

    private void OnEnable() {
        _rectTransform = GetComponent<RectTransform>();
        _buttonHeight = _buttonPrefab.GetComponent<RectTransform>().sizeDelta.y;
        _playerLineController = GetComponentInParent<PlayerLineController>();
        _player = _playerLineController.Player;
        GetComponent<Image>().material = TeamMaterialManager.Singleton.GetFlatUI(_player.TeamID);

        _menuOptions = _player.IsBot ? _botOptions : _options;

        StartCoroutine(OpenMenu());
    }

    /// <summary>
    /// Closing the menu wherever we press
    /// </summary>
    private void Update() {
        if (Input.GetMouseButtonUp(0))
            StartCoroutine(CloseMenu());
    }

    private IEnumerator OpenMenu() {
        Vector2 sizeDelta = _rectTransform.sizeDelta;
        float finalHeight = sizeDelta.y + _buttonHeight * _menuOptions.Length;

        var finalSize = new Vector2(sizeDelta.x, finalHeight);
        var finalPosition = new Vector2(_rectTransform.anchoredPosition.x, finalSize.y);
        _rectTransform.sizeDelta = Vector2.zero;

        var buttonCount = 0;

        while (Vector2.Distance(_rectTransform.sizeDelta, finalSize) >= 1) {
            //Add button and event to onclick
            //Adding a new button, then the sizeDelta is higher than the height of the current button count
            if (_rectTransform.sizeDelta.y >= _buttonHeight * buttonCount + FrameThickness) {
                GameObject newButton = InstantiateWrapper.InstantiateWithMessage(_buttonPrefab, _optionList.transform);

                //Configure the Button
                var optionButton = newButton.GetComponent<PlayerOptionButton>();
                optionButton.Player = _player;
                optionButton.TeamColor = TeamManager.Singleton.Get(_player.TeamID).Colors.UI;
                optionButton.PlayerOption = _menuOptions[buttonCount];
                _buttons.Add(newButton);
                buttonCount++;
            }

            _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, finalPosition, LerpSpeed);
            _rectTransform.sizeDelta = Vector2.Lerp(_rectTransform.sizeDelta, finalSize, LerpSpeed);
            yield return null;
        }

        _rectTransform.anchoredPosition = finalPosition;
        _rectTransform.sizeDelta = finalSize;
    }


    private IEnumerator CloseMenu() {
        Vector2 sizeDelta = _rectTransform.sizeDelta;
        float finalHeight = sizeDelta.y - _buttonHeight * _menuOptions.Length;
        var finalSize = new Vector2(sizeDelta.x, finalHeight);
        var finalPosition = new Vector2(_rectTransform.anchoredPosition.x, finalSize.y);

        int buttonCount = _menuOptions.Length;

        while (Vector2.Distance(_rectTransform.sizeDelta, Vector2.zero) >= 1) {
            if (_rectTransform.sizeDelta.y <= _buttonHeight * buttonCount - FrameThickness) {
                if(buttonCount - 1 >= 0 && buttonCount - 1 <= _buttons.Count)
                    Destroy(_buttons[buttonCount - 1]);
                else
                    Debug.LogWarning($"Index out fo Range! The button number {buttonCount - 1} is not part of the array! Cant be destroyed");
                buttonCount--;
            }

            _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, finalPosition, LerpSpeed);
            _rectTransform.sizeDelta = Vector2.Lerp(_rectTransform.sizeDelta, Vector2.zero, LerpSpeed);
            yield return null;
        }

        Destroy(gameObject);
    }
}