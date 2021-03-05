using TowerTag;
using UnityEngine;

public class CustomizationDebugUI : MonoBehaviour {
    private bool _isVisible;

    private void Awake() {
        if (!Application.isEditor && !Debug.isDebugBuild) enabled = false;
    }

    private void Update() {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C)) _isVisible = !_isVisible;
    }

    private void OnGUI() {
        if (!_isVisible) return;

        if (!Customization.UseCustomLogos && GUILayout.Button("Use custom logos")) {
            Customization.Instance.CustomizeLogos(true);
        }

        if (Customization.UseCustomLogos && GUILayout.Button("Don't Use custom logos")) {
            Customization.Instance.CustomizeLogos(false);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Fire Hue:");
        ConfigurationManager.Configuration.FireHue =
            ushort.Parse(GUILayout.TextField(ConfigurationManager.Configuration.FireHue.ToString()));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Ice Hue:");
        ConfigurationManager.Configuration.IceHue =
            ushort.Parse(GUILayout.TextField(ConfigurationManager.Configuration.IceHue.ToString()));
        GUILayout.EndHorizontal();

        if (!Customization.UseCustomColors && GUILayout.Button("Use custom colors")) {
            Customization.Instance.CustomizeColors(TeamManager.Singleton);
        }

        if (Customization.UseCustomColors && GUILayout.Button("Update custom colors")) {
            Customization.Instance.CustomizeColors(TeamManager.Singleton);
        }

        if (Customization.UseCustomColors && GUILayout.Button("Use default colors")) {
            Customization.Instance.UseDefaultColors(TeamManager.Singleton);
        }
    }
}