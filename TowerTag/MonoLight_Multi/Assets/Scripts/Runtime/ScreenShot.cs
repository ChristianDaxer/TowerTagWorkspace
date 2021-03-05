using UnityEngine;
using UnityEngine.Serialization;

public class ScreenShot : MonoBehaviour {
    private int _count;

    [FormerlySerializedAs("cameraVR")] [SerializeField]
    private GameObject _cameraVR;

    [FormerlySerializedAs("camera2DScreenshot")] [SerializeField]
    private GameObject _camera2DScreenshot;

    private void Start() {
        _count = 0;
    }

    private void Update() {
        if (Input.GetKey("c")) {
            _camera2DScreenshot.SetActive(true);
            _camera2DScreenshot.transform.position = _cameraVR.transform.position;
            _camera2DScreenshot.transform.rotation = _cameraVR.transform.rotation;
            var renTex = new RenderTexture(4320, 2400, 24, RenderTextureFormat.ARGB32);
            renTex.Create();
            var cam = _camera2DScreenshot.GetComponent<Camera>();
            cam.targetTexture = renTex;
            cam.fieldOfView = 60;
            cam.Render();
            RenderTexture.active = renTex;
            var screenshotTex = new Texture2D(renTex.width, renTex.height, TextureFormat.RGB24, false);
            screenshotTex.ReadPixels(new Rect(0, 0, renTex.width, renTex.height), 0, 0);
            screenshotTex.Apply();
            RenderTexture.active = null;
            System.IO.File.WriteAllBytes("Screenshots/" + _count + ".png", screenshotTex.EncodeToPNG());
            Destroy(renTex);
            Destroy(screenshotTex);

            ++_count;
            _camera2DScreenshot.SetActive(false);


            print("screenShot taken");
        }
    }
}