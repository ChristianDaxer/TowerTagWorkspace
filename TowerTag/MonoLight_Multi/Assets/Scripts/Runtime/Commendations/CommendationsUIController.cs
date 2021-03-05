using UI;
using UnityEngine;

namespace Commendations {
    /// <summary>
    /// UI Controller for the operator UI within the commendations scene.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public class CommendationsUIController : MonoBehaviour {
        [SerializeField] private MessageQueue _messageQueue;
        public Camera ScreenshotCamera { get; set; }

        /// <summary>
        /// Open a popup that queries the user to confirm taking a screenshot and printing one copy per player
        /// using the default printer of the operating system.
        /// </summary>
        public void OnScreenshotButtonClicked() {
            if (ScreenshotCamera == null) {
                Debug.LogError("Cant find Screenshot camera. Print aborted");
                return;
            }
            int amountOfCopies = PlayerManager.Instance.GetParticipatingPlayersCount();
            _messageQueue.AddYesNoMessage(
                $"This will take a screenshot and print {amountOfCopies} copies using your default printer.",
                "Print Screenshot", null, null, "PRINT", ConfirmScreenshot, "CANCEL");
        }

        /// <summary>
        /// Return to the hub scene immediately.
        /// </summary>
        public void GoBackToHub() {
            GameManager.Instance.TriggerMatchConfigurationOnMaster();
        }

        public void GoToOffboardingScene() {
            GameManager.Instance.LoadOffboarding();
        }

        private void ConfirmScreenshot() {
            int copies = PlayerManager.Instance.GetParticipatingPlayersCount();
            Texture2D screenshot = TakeScreenshot();
            PrintImage(screenshot, copies);
        }

        private Texture2D TakeScreenshot() {
            RenderTexture tempRenderTexture = RenderTexture.active;
            var screenshotRenderTexture = new RenderTexture(1920 * 4, 1080 * 4, 32) {
                antiAliasing = 8,
                filterMode = FilterMode.Trilinear
            };
            Debug.Log("Taking screenshot with a resolution of width/height = " +
                      $"{screenshotRenderTexture.width}/{screenshotRenderTexture.height}.");

            ScreenshotCamera.targetTexture = screenshotRenderTexture;
            RenderTexture.active = ScreenshotCamera.targetTexture;
            ScreenshotCamera.Render();
            RenderTexture targetTexture = ScreenshotCamera.targetTexture;
            var screenshot = new Texture2D(targetTexture.width, targetTexture.height);
            screenshot.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = tempRenderTexture;
            return screenshot;
        }

        private static void PrintImage(Texture2D image, int copies) {
            Debug.Log($"Printing {copies} copies of image with width/height = {image.width}/{image.height}");
            LCPrinter.Print.PrintTexture(image.EncodeToPNG(), copies, null);
        }
    }
}