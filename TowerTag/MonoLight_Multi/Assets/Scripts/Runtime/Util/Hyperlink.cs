using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Hyperlink : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Text _buttonText;
    [SerializeField] private string _displayText;
    [SerializeField] private string _url;

    public string Url
    {
        set => _url = value;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OpenHyperlink);
        if(_buttonText != null && _displayText != "")
            _buttonText.text = _displayText;
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OpenHyperlink);
    }

    private void OpenHyperlink()
    {
        Application.OpenURL(_url);
    }
}
