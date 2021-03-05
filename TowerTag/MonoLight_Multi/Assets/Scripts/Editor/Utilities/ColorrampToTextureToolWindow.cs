using UnityEngine;
using UnityEditor;

public class ColorrampToTextureToolWindow : EditorWindow {
    // Gradient
    [SerializeField] private Gradient _gradient;

    // Texture
    [SerializeField] private int _texWidth = 256;
    [SerializeField] private int _texHeight = 1;
    [SerializeField] private TextureFormat _texFormat = TextureFormat.RGBA32;
    [SerializeField] private FilterMode _texFilter = FilterMode.Bilinear;
    [SerializeField] private Texture2D _texture;

    // GradientUI
    [SerializeField] private SerializedProperty _gradientProperty;
    [SerializeField] private SerializedObject _gradientObject;

    private readonly GUIContent _gradientLabel = new GUIContent("ColorRamp: ", "Set Color-/Alpha-Keys to create ramp.");

    // Texture UI
    private const int TexPreviewHeight = 10;
    private int _texPreviewUIOffset;

    private readonly GUIContent _widthLabel = new GUIContent("TextureWidth: ", "Set number of pixels in X-dimension.");

    private readonly GUIContent
        _heightLabel = new GUIContent("TextureHeight: ", "Set number of pixels in Y-dimension.");

    private readonly GUIContent _formatLabel = new GUIContent("TextureFormat: ", "Set texture format.");
    private readonly GUIContent _filterLabel = new GUIContent("TextureFilter: ", "Set texture filter mode.");

    // Save UI
    [SerializeField] private string _lastPath = "Assets";
    [SerializeField] private string _fileName = "rampTex";


    [MenuItem("PillarGame/ColorRampToTextureTool")]
    private static void Init() {
        var win = (ColorrampToTextureToolWindow) GetWindow(typeof(ColorrampToTextureToolWindow));
        win.Show();
    }

    private void OnGUI() {
        // create Serialized property from gradient member (to draw Unity's Gradient window), needed only once but don't want to make the members static -> so we do it every frame twice (America Fuck Yeah!)
        _gradientObject = new SerializedObject(this);
        _gradientProperty = _gradientObject.FindProperty("_gradient");

        // Draw Headline Label
        EditorGUILayout.LabelField(_gradientLabel, EditorStyles.boldLabel);

        // draw Gradient field (window to manipulate color ramp)
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_gradientProperty, _gradientLabel, null);
        if (EditorGUI.EndChangeCheck())
            _gradientObject.ApplyModifiedProperties();

        // draw Texture preview
        if (_texture != null) {
            DrawPreviewTexture();
        }

        // draw some empty space
        _texPreviewUIOffset = (_texture != null) ? TexPreviewHeight : 0;
        GUILayout.Space(10 + _texPreviewUIOffset);

        // show Texture settings
        EditorGUILayout.LabelField("Texture settings: ", EditorStyles.boldLabel);
        _texWidth = EditorGUILayout.IntField(_widthLabel, _texWidth);
        _texHeight = EditorGUILayout.IntField(_heightLabel, _texHeight);
        _texFormat = (TextureFormat) EditorGUILayout.EnumPopup(_formatLabel, _texFormat);
        _texFilter = (FilterMode) EditorGUILayout.EnumPopup(_filterLabel, _texFilter);

        // Create Texture
        if (GUILayout.Button("Apply color ramp to Texture")) {
            ApplyRampToTexture();
        }

        // Save Texture
        if (GUILayout.Button("Save Texture")) {
            SaveTexture();
        }
    }

    private void DrawPreviewTexture() {
        // divide rect in two to draw Label and tex preview in same layout as the color ramp
        Rect rect = EditorGUILayout.GetControlRect();
        float rectWidth = rect.width;

        // second rect (0, y, 150, height)
        rect.width = 150;

        // draw label
        EditorGUI.LabelField(rect, new GUIContent("Texture preview: ", "Preview of created Texture."));

        // second rect (150, y, width - 150, height)
        rect.x += rect.width;
        rect.width = rectWidth - rect.x;

        // draw tex
        EditorGUI.DrawTextureTransparent(rect, _texture);
    }

    // Create new Texture and apply color ramp values
    private void ApplyRampToTexture() {
        if (_texture != null) {
            DestroyImmediate(_texture);
        }

        _texture = ColorRampToTexture.ConvertColorRampToTexture(_texWidth, _texHeight, _texFormat, _gradient,
            _texFilter);
    }

    // save current Texture
    private void SaveTexture() {
        ApplyRampToTexture();

        string fileName = _fileName + "_" + _texWidth + "by" + _texHeight + "pix";
        string tmpPath = EditorUtility.SaveFilePanel("Save Texture", _lastPath, fileName, "png");

        // check returned path, if null the dialog was canceled
        if (string.IsNullOrEmpty(tmpPath)) {
            Debug.LogError("Abort Saving Asset to disk: Path from FileDialog is not valid!");
            return;
        }

        // remember last save
        _lastPath = tmpPath;

        // encode texture & save to disk
        byte[] texBytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(tmpPath, texBytes);

        // refresh project view
        AssetDatabase.Refresh();
    }
}