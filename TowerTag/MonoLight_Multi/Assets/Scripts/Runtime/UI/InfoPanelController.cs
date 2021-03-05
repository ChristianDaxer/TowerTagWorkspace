using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InfoPanelController : MonoBehaviour {
    [SerializeField] private float _buttonZOffset;
    [SerializeField] private Button[] _button;
    [SerializeField] private Transform _infoPanel;
    [SerializeField] private RawImage _infoImage;

    [FormerlySerializedAs("_url")] [SerializeField]
    private string _imageUrl;

    [SerializeField] private string _linkUrl;

    private Vector3 _infoPanelStartPosition;

    private void OnEnable() {
        _button.ForEach(button => button.interactable = true);
    }

    private void Start() {
        _infoPanelStartPosition = _infoPanel.localPosition;
        _infoImage.GetComponent<Hyperlink>();
        StartCoroutine(DownloadImage(_imageUrl));
        StartCoroutine(GetLink(_linkUrl));
    }

    IEnumerator DownloadImage(string mediaUrl) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            Texture2D srcNoMipmaps = ((DownloadHandlerTexture) request.downloadHandler).texture;
            // Add mipmaps to avoid aliasing and reduce stress on memory bandwidth for the fragment shader
            // TODO: In order to save memory, the texture could be resized instead to match display resolution 
            if (srcNoMipmaps != null)
            {
                Texture2D dstWithMipmaps =
                    new Texture2D(srcNoMipmaps.width, srcNoMipmaps.height, srcNoMipmaps.format, true);
                dstWithMipmaps.SetPixels32(srcNoMipmaps.GetPixels32(0), 0);
                dstWithMipmaps.Apply(true);
                _infoImage.texture = dstWithMipmaps;
            }
        }
    }

    IEnumerator GetLink(string url) {
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) {
            Debug.Log(request.error);
        }
        else {
            // Show results as text
            _infoImage.GetComponent<Hyperlink>().Url = request.downloadHandler.text;
        }
    }

    [UsedImplicitly]
    public void OnPointerEnter() {
        foreach (var button in _button) {
            if (button.interactable)
                button.transform.localPosition += new Vector3(0, 0, _buttonZOffset);
        }
    }

    [UsedImplicitly]
    public void OnPointerExit() {
        foreach (var button in _button) {
            if (button.interactable)
                button.transform.localPosition -= new Vector3(0, 0, _buttonZOffset);
        }
    }

    [UsedImplicitly]
    public void OnInfoPanelButtonClicked(Button clickedButton) {
        var b = _button.FirstOrDefault(button => button == clickedButton);
        if (b != null) {
            b.interactable = false;
        }

        _infoPanel.localPosition = _infoPanelStartPosition;
    }
}