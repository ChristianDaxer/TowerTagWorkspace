using System;
using System.Collections.Generic;
using Home.UI;
using JetBrains.Annotations;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class OverlayCanvasModel : Logger {
    [SerializeField, Tooltip("The headline text of the message box.")]
    private Text _messageHeadText;

    [SerializeField, Tooltip("The body text of the message box.")]
    private Text _messageBodyText;

    [SerializeField, Tooltip("The Button which closes the message box")]
    private Button _closeButton;

    [SerializeField, Tooltip("The Prefab from which is used to instantiate message buttons")]
    private Button _buttonPrefab;

    [SerializeField, Tooltip("Container transform for buttons")]
    private Transform _buttonContainer;

    [SerializeField, Tooltip("The Prefab from which is used to instantiate message buttons")]
    private TMP_InputField _inputFieldPrefab;

    [SerializeField, Tooltip("Container transform for the input field")]
    private Transform _inputFieldContainer;

    private TMP_InputField _inputField;
    [SerializeField] private BoxCollider _messageCollider;

    public Canvas Canvas { get; private set; }

    /// <summary>
    /// This event is fired when the overlay canvas is closed.
    /// </summary>
    public event Action OnClose;

    /// <summary>
    /// This event is fired when the overlay canvas is opened.
    /// </summary>
    public event Action OnOpen;

    protected new void Awake() {
        base.Awake();
        Canvas = GetComponent<Canvas>();
    }

    /// <summary>
    /// Pop up a windows with the configuration
    /// </summary>
    /// <param name="header">Caption</param>
    /// <param name="body">Content</param>
    /// <param name="closeable"></param>
    /// <param name="buttons"></param>
    /// <param name="inputFields"></param>
    public void ShowMessage(string header, string body, bool closeable = false,
        [CanBeNull] List<MessageButton> buttons = null, List<MessageInputField> inputFields = null) {
        _messageHeadText.text = header;
        _messageBodyText.text = body;
        Closeable = closeable;

        foreach (Transform child in _inputFieldContainer) {
            Destroy(child.gameObject);
        }

        inputFields?.ForEach(AddInputfield);

        foreach (Transform child in _buttonContainer) {
            Destroy(child.gameObject);
        }

        buttons?.ForEach(AddButton);

        if(_messageCollider != null) _messageCollider.enabled = true;
        Canvas.enabled = true;
        OnOpen?.Invoke();
    }

    /// <summary>
    /// Add a button to the message
    /// </summary>
    /// <param name="messageButton"></param>
    private void AddButton(MessageButton messageButton) {
        Button button = InstantiateWrapper.InstantiateWithMessage(_buttonPrefab, _buttonContainer);
        button.onClick.AddListener(Close);


        if (_inputField != null && messageButton.ClickedWithText != null)
            button.onClick.AddListener(() => messageButton.ClickedWithText(_inputField.text));

        if (messageButton.Clicked != null) {
            button.onClick.AddListener(messageButton.Clicked);
        }

        button.GetComponentInChildren<Text>().text = messageButton.Text;
    }

    /// <summary>
    /// Add an inputfield dynamically to the message
    /// </summary>
    /// <param name="messageInputField"></param>
    private void AddInputfield(MessageInputField messageInputField) {
        _inputField = InstantiateWrapper.InstantiateWithMessage(_inputFieldPrefab, _inputFieldContainer);
        InputFieldHelper.SetInputFieldSettings(_inputField, messageInputField.Type);
        _inputField.text = messageInputField.Text;
        _inputField.placeholder.GetComponent<TMP_Text>().text = messageInputField.Placeholder;
        if (messageInputField.IsValid != null) {
            _inputField.textComponent.color = messageInputField.IsValid(_inputField.text) ? Color.white : Color.red;
            _inputField.onValueChanged.AddListener(input =>
                    _inputField.textComponent.color = messageInputField.IsValid(input) ? Color.white : Color.red
                );
        }
    }

    /// <summary>
    /// Close button function -> closing the window
    /// </summary>
    public void Close() {
        Hide();
        OnClose?.Invoke();
    }

    public void Hide() {
        if(_messageCollider != null) _messageCollider.enabled = false;
        Canvas.enabled = false;
    }

    private bool Closeable {
        set => _closeButton.gameObject.SetActive(value);
    }
}