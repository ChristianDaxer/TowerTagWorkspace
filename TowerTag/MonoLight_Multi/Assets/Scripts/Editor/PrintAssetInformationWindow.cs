using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;

public class PrintAssetInformationWindow : EditorWindow
{
    public enum SortingMode
    {
        DiagonalSize,
        CompressionSize
    }

    public enum AssetType
    {
        None,
        Texture,
        Mesh
    }

    public enum SearchMode
    {
        FILTERMODE_ALL = 0,
        FILTERMODE_NAME = 1,
        FILTERMODE_TYPE = 2
    }

    private class ObjectDetail
    {
        public string name;
        public int assetGuid;
    }
    private string[] textures = null;
    private Vector2 scrollPosition;
    private SortingMode sortingMode;
    private AssetType selectAssetType = AssetType.Mesh;

    // private EditorBuildSettingsScene[] Scenes = EditorBuildSettings.scenes;
    private Dictionary<int, string> sceneNames = new Dictionary<int, string>();
    private Dictionary<int, string[]> sceneDependencies = new Dictionary<int, string[]>();
    private Dictionary<int, string> scenePaths = new Dictionary<int, string>();

    private Dictionary<string, int[]> texturesInScene = new Dictionary<string, int[]>();
    private Dictionary<string, int[]> meshesInScene = new Dictionary<string, int[]>();
    private Dictionary<string, int[]> materialsInScene = new Dictionary<string, int[]>();
    private int selectSceneIndex = -1;
    private int prevSelectSceneIndex = -1;
    private Scene openScene = new Scene();




    // Loaded materials information 
    private Material[] materialRefs = null;
    private string[] materialNames = null;
    private string[] materialPaths = null;
    private string materialInScenes = null;

    private Mesh[] meshRefs = null;
    private string[] meshPaths = null;
    private string[] meshNames = null;
    private int prevSelectAssetIndex = -1;
    private int selectAssetIndex = -1;

    // Loaded textures information 
    private string[] texturePaths = null;
    private string[] textureNames = null;

    private List<int> targetObjects = new List<int>();
    private string[] targetGameObjects = null;
    private List<UnityEngine.Object> targetObject = new List<UnityEngine.Object>();
    private List<UnityEngine.Object> targetList = new List<UnityEngine.Object>();

    private Dictionary<string, string[]> materialToTextures = new Dictionary<string, string[]>();
    private Dictionary<string, List<KeyValuePair<string, int>>> textureToMaterials = new Dictionary<string, List<KeyValuePair<string, int>>>();
    private Dictionary<int, ObjectDetail[]> sceneToMeshObjects = new Dictionary<int, ObjectDetail[]>();
    private Dictionary<int, ObjectDetail[]> sceneToMaterialObjects = new Dictionary<int, ObjectDetail[]>();
    private bool analyze = false;

    [MenuItem("Unity/Print Asset Information")]
    private static void Open()
    {
        PrintAssetInformationWindow window = PrintAssetInformationWindow.GetWindow<PrintAssetInformationWindow>();
        window.Show();
    }

    private string[] FindAssetPaths(string filter)
    {
        var guids = AssetDatabase.FindAssets(filter);
        List<string> paths = new List<string>();
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            paths.Add(path);
        }
        return paths.ToArray();

    }

    private string[] FindAllMaterial()
    {
        var guids = AssetDatabase.FindAssets("t:Material");
        List<string> paths = new List<string>();
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            paths.Add(path);
        }
        return paths.ToArray();
    }

    private void LoadMeshes()
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int i = 0; i < meshPaths.Length; i++)
        {
            string path = meshPaths[i];
            var m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (m != null)
                meshes.Add(m);
        }

        meshRefs = meshes.ToArray();
        meshNames = new string[meshPaths.Length];
        for (int i = 0; i < meshPaths.Length; i++)
        {
            meshNames[i] = meshPaths[i].Substring(CutName(meshPaths[i]));
        }
    }

    private void LoadMaterials()
    {
        List<Material> mat = new List<Material>();
        for (int i = 0; i < materialPaths.Length; i++)
        {
            string path = materialPaths[i];
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null)
                mat.Add(m);
        }

        materialRefs = mat.ToArray();
        materialNames = new string[materialPaths.Length];
        for (int i = 0; i < materialPaths.Length; i++)
        {
            materialNames[i] = materialPaths[i].Substring(CutName(materialPaths[i]));
        }
    }

    private void LoadTextures()
    {
        List<string> paths = new List<string>();
        materialToTextures.Clear();
        for (int i = 0; i < materialRefs.Length; i++)
        {
            List<string> path = GetTextureFromMaterial(materialRefs[i], materialPaths[i]);
            if (path != null && path.Count > 0)
            {
                paths.AddRange(path);
            }

            if (!materialToTextures.ContainsKey(materialNames[i]))
                materialToTextures.Add(materialNames[i], path.ToArray());
        }

        texturePaths = paths.Distinct().ToArray();

        textureNames = new string[texturePaths.Length];
        for (int i = 0; i < texturePaths.Length; i++)
        {
            textureNames[i] = texturePaths[i].Substring(CutName(texturePaths[i]));
        }

    }

    private List<string> GetTextureFromMaterial(Material mat, string matPath)
    {
        List<string> results = new List<string>();
        UnityEngine.Object[] roots = new UnityEngine.Object[] { mat };
        var dependObjs = EditorUtility.CollectDependencies(roots);
        foreach (UnityEngine.Object dependObj in dependObjs)
        {
            if (dependObj.GetType() == typeof(Texture2D))
            {
                string textPath = AssetDatabase.GetAssetPath(dependObj.GetInstanceID());
                if (!textPath.Contains("Resources"))
                {
                    results.Add(textPath);
                    int insId = mat.GetInstanceID();
                    string guid = "";
                    long localId = -1;
                    bool getGuid = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(insId, out guid, out localId);
                    string textName = textPath.Substring(CutName(textPath));
                    if (!textureToMaterials.ContainsKey(textName))
                        textureToMaterials.Add(textName, new List<KeyValuePair<string, int>>() { new KeyValuePair<string, int>(matPath, guid.GetHashCode()) });
                    else
                        textureToMaterials[textName].Add(new KeyValuePair<string, int>(matPath, guid.GetHashCode()));
                }
            }
        }
        return results;
    }

    private void AnalyzeAssets()
    {
        analyze = true;
        targetGameObjects = null;
        string[] allMeshPaths = FindAssetPaths("t:Mesh");
        string[] allMatPaths = FindAssetPaths("t:Material");
        List<string> meshsInScene = new List<string>();
        List<string> matsInScene = new List<string>();
        prevSelectAssetIndex = -2;
        selectAssetIndex = -1;
        prevSelectSceneIndex = -2;
        selectSceneIndex = -1;

        // Get the number of scenes in the build settings.
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        sceneNames.Clear();
        sceneDependencies.Clear();
        scenePaths.Clear();

        // Loop through the scenes.
        for (int si = 0; si < sceneCount; si++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(si);
            scenePaths.Add(si, scenePath);
            sceneNames.Add(si, GetSceneNames(scenePath));

            // Get all dependencies required by scene.
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (sceneDependencies != null)
            {
                sceneDependencies.Add(si, dependencies);
            }

            // Intersect all scene dependencies with all textures to just give you textures used in the scene.        
            var materialsRefToScene = dependencies.Intersect(allMatPaths);
            var meshRefToScene = dependencies.Intersect(allMeshPaths);

            if (materialsRefToScene.Count() > 0)
                matsInScene.AddRange(materialsRefToScene);

            if (meshRefToScene.Count() > 0)
                meshsInScene.AddRange(meshRefToScene);
        }


        materialPaths = matsInScene.Distinct().ToArray();
        meshPaths = meshsInScene.Distinct().ToArray();
        LoadMaterials();
        LoadTextures();
        LoadMeshes();
        SortMeshes();

        AnalyzeAssetsInScene(texturePaths, textureNames, ref texturesInScene);
        AnalyzeAssetsInScene(meshPaths, meshNames, ref meshesInScene);
        AnalyzeAssetsInScene(materialPaths, materialNames, ref materialsInScene);
        FindAssetsInScenes();
        SetSearchFilter("", SearchMode.FILTERMODE_NAME);
    }



    private void AnalyzeAssetsInScene(string[] assetPaths, string[] assetNames, ref Dictionary<string, int[]> assetInScene)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        // Analyze textures in every scene
        for (int i = 0; i < assetPaths.Length; i++)
        {
            string assetName = assetNames[i];
            if (assetInScene.ContainsKey(assetName))
                continue;

            List<int> list = new List<int>();
            for (int si = 0; si < sceneCount; si++)
            {
                if (sceneDependencies.ContainsKey(si))
                {
                    string[] dependencies = sceneDependencies[si];
                    for (int j = 0; j < dependencies.Length; j++)
                    {
                        if (dependencies[j].Contains(assetName))
                        {
                            list.Add(si);
                            break;
                        }
                    }
                }
            }
            assetInScene.Add(assetName, list.ToArray());
        }
    }

    void FindAssetsInScenes()
    {
        sceneToMaterialObjects.Clear();
        sceneToMeshObjects.Clear();
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int si = 0; si < sceneCount; si++)
        {
            var currentScene = EditorSceneManager.OpenScene(scenePaths[si]);
            GameObject[] roots = currentScene.GetRootGameObjects();
            List<ObjectDetail> meshObjs = new List<ObjectDetail>();
            List<ObjectDetail> matObjs = new List<ObjectDetail>();
            for (int i = 0; i < roots.Length; i++)
            {
                // FindObjectsOfType<Mesh>();
                MeshFilter[] filters = roots[i].GetComponentsInChildren<MeshFilter>(true);
                for (int j = 0; j < filters.Length; j++)
                {

                    if (filters[j].sharedMesh != null)
                    {
                        string guid = null;
                        long localId = -1;
                        int insId = filters[j].sharedMesh.GetInstanceID();
                        bool getGuid = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(insId, out guid, out localId);
                        if (getGuid)
                        {
                            ObjectDetail obj = new ObjectDetail();
                            obj.name = filters[j].gameObject.name;
                            obj.assetGuid = guid.GetHashCode();
                            meshObjs.Add(obj);
                        }
                    }
                }

                Renderer[] renderers = roots[i].GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renderers.Length; j++)
                {
                    Material[] mats = renderers[j].sharedMaterials;
                    if (mats == null)
                        continue;

                    for (int k = 0; k < mats.Length; k++)
                    {
                        if (mats[k] == null)
                            continue;

                        string guid = null;
                        long localId = -1;
                        int insId = mats[k].GetInstanceID();
                        bool getGuid = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(insId, out guid, out localId);
                        if (getGuid)
                        {
                            ObjectDetail obj = new ObjectDetail();
                            obj.name = renderers[j].gameObject.name;
                            obj.assetGuid = guid.GetHashCode();
                            matObjs.Add(obj);
                        }
                    }
                }
            }

            if (!sceneToMeshObjects.ContainsKey(si) && meshObjs.Count() > 0)
            {
                var objects = meshObjs.Distinct().ToArray();
                sceneToMeshObjects.Add(si, objects);
            }

            if (!sceneToMaterialObjects.ContainsKey(si) && matObjs.Count() > 0)
            {
                var objects = matObjs.Distinct().ToArray();
                // Debug.Log("Scene " + si + " has " +objects.Length+" assets!");
                sceneToMaterialObjects.Add(si, objects);
            }

            EditorSceneManager.CloseScene(currentScene, true);
        }
    }
    private void SelectAssetType()
    {
        AssetType newType = (AssetType)EditorGUILayout.EnumPopup("Asset Type:", selectAssetType);
        if (selectAssetType != newType)
        {
            prevSelectAssetIndex = -1;
            selectAssetIndex = -2;
            prevSelectSceneIndex = -1;
            selectSceneIndex = -1;
        }

        selectAssetType = newType;
    }

    private void SelectSortMode()
    {
        // Sorting Texture
        SortingMode newSortingMode = (SortingMode)EditorGUILayout.EnumPopup("Sort:", sortingMode);

        if (newSortingMode != sortingMode)
        {
            switch (sortingMode)
            {
                // texture paths are sorted by width/height of loaded texture
                case SortingMode.DiagonalSize:
                    {
                        texturePaths = texturePaths.OrderBy(texturePath =>
                        {
                            if (AssetDatabase.LoadAssetAtPath<Texture>(texturePath).height > 0)
                                return AssetDatabase.LoadAssetAtPath<Texture>(texturePath).width / AssetDatabase.LoadAssetAtPath<Texture>(texturePath).height;
                            else
                                return 0;
                        }).ToArray();

                    }
                    break;

                // sort by texture size
                case SortingMode.CompressionSize:
                    {

                        texturePaths = texturePaths.OrderByDescending(texturePath =>
                        {
                            return AssetDatabase.LoadAssetAtPath<Texture>(texturePath).height *
                            AssetDatabase.LoadAssetAtPath<Texture>(texturePath).width;
                        }).ToArray();
                      
                    }
                    break;
                default:
                    break;
            }
            sortingMode = newSortingMode;
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Analyze Assets"))
            AnalyzeAssets();

        if (!analyze)
            return;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        SelectAssetType();

        switch (selectAssetType)
        {
            case AssetType.Texture:
                {
                    if (texturePaths != null)
                    {
                        {
                            SelectSortMode();
                            for (int ti = 0; ti < texturePaths.Length; ti++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                SelectTexture(ti);
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                SelectMaterial(ti);
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginVertical();
                                string path = texturePaths[ti];
                                int insId = AssetDatabase.LoadAssetAtPath<Texture2D>(path).GetInstanceID();
                                long localID = -1;
                                string guid = null;
                                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(insId, out guid, out localID);
                                SelectScene(ti, textureNames[ti], guid, texturesInScene, sceneToMaterialObjects, textureToMaterials);
                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                }
                break;
            case AssetType.Mesh:
                {
                    if (meshPaths != null)
                    {
                        for (int i = 0; i < meshPaths.Length; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            SelectMesh(i);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginVertical();
                            string path = meshPaths[i];
                            int insId = AssetDatabase.LoadAssetAtPath<Mesh>(path).GetInstanceID();
                            long localID = -1;
                            string guid = null;
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(insId, out guid, out localID);
                            SelectScene(i, meshNames[i], guid, meshesInScene, sceneToMeshObjects);

                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                break;
            default:
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    int CutName(string name)
    {
        int cut = 0;
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (name[i].Equals('/'))
            {
                cut = i;
                return cut + 1;
            }
        }
        return cut;
    }

    private void SelectMaterial(int index)
    {
        string name = textureNames[index];
        if (textureToMaterials.ContainsKey(name) && textureToMaterials[name].Count > 0)
        {
            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.red;
            for (int i = 0; i < textureToMaterials[name].Count; i++)
            {
                style.normal.textColor = Color.red;
                if (selectAssetIndex == index)
                    style.normal.textColor = Color.magenta;

                string matPath = textureToMaterials[name][i].Key;
                string matName = matPath.Substring(CutName(matPath));
                bool selectMaterial = GUILayout.Button(matName, style, GUILayout.Width(10 * matName.Length));
                if (selectMaterial)
                {
                    Selection.objects = new UnityEngine.Object[1] { AssetDatabase.LoadAssetAtPath<Material>(matPath) };
                }
            }
        }
    }

    private void SortMeshes()
    {
        meshPaths = meshPaths.OrderBy(meshPath =>
        {
            return AssetDatabase.LoadAssetAtPath<Mesh>(meshPath).vertexCount;
        }).ToArray();


        meshRefs = meshRefs.OrderBy(meshRef =>
        {
            return meshRef.vertexCount;
        }).ToArray();

        for (int i = 0; i < meshPaths.Length; i++)
        {
            meshNames[i] = meshPaths[i].Substring(CutName(meshPaths[i]));
        }
    }

    private void SelectMesh(int index)
    {
        var style = new GUIStyle(GUI.skin.button);

        style.normal.textColor = Color.black;
        if (index == selectAssetIndex)
            style.normal.textColor = Color.yellow;

        bool selectMesh = GUILayout.Button("Select", style, GUILayout.Width(50));
        EditorGUILayout.LabelField(meshNames[index]);
        // Show in inspector
        if (selectMesh)
        {
            Selection.objects = new UnityEngine.Object[1] { AssetDatabase.LoadAssetAtPath<Mesh>(meshPaths[index]) };
            prevSelectAssetIndex = selectAssetIndex;
            selectAssetIndex = index;
            if (prevSelectAssetIndex != selectAssetIndex)
                targetGameObjects = null;
        }
    }

    private bool SelectTexture(int index)
    {
        var style = new GUIStyle(GUI.skin.button);

        style.normal.textColor = Color.black;
        if (index == selectAssetIndex)
            style.normal.textColor = Color.yellow;

        bool selectTexture = GUILayout.Button("Select", style, GUILayout.Width(50));

        for (int i = texturePaths[index].Length - 1; i >= 0; i--)
        {
            if (texturePaths[index][i].Equals('/') && textureNames != null)
            {
                textureNames[index] = texturePaths[index].Substring(i + 1);
                break;
            }
        }

        EditorGUILayout.LabelField(textureNames[index] + " is used by materials: ");

        // Show in inspector
        if (selectTexture)
        {
            Selection.objects = new UnityEngine.Object[1] { AssetDatabase.LoadAssetAtPath<Texture>(texturePaths[index]) };
            prevSelectAssetIndex = selectAssetIndex;
            selectAssetIndex = index;
            if (prevSelectAssetIndex != selectAssetIndex)
                targetGameObjects = null;
        }
        return selectTexture;
    }

    private bool SelectScene(int searchAssetIndex, string assetName, string assetGuid,
        Dictionary<string, int[]> assetsInScene,
        Dictionary<int, ObjectDetail[]> dataBase,
                Dictionary<string, List<KeyValuePair<string, int>>> assetInAssets = null)
    {
        if (!assetsInScene.ContainsKey(assetName))
            return false;

        EditorGUILayout.LabelField("in scenes: ");
        var style = new GUIStyle(GUI.skin.button);
        int sceneCount = assetsInScene[assetName].Length;
        bool selectScene = false;
        for (int i = 0; i < sceneCount; i++)
        {
            int sceneIndex = assetsInScene[assetName][i];
            style.normal.textColor = Color.blue;
            if (searchAssetIndex == selectAssetIndex && sceneIndex == selectSceneIndex)
                style.normal.textColor = Color.white;

            selectScene = GUILayout.Button(sceneNames[sceneIndex], style, GUILayout.Width(10 * sceneNames[sceneIndex].Length));
            var scenes = assetsInScene[assetName];
            List<KeyValuePair<string, int>> list = null;
            if (assetInAssets != null)
                list = assetInAssets[assetName];

            if (selectScene)
            {
                prevSelectSceneIndex = selectSceneIndex;
                selectSceneIndex = sceneIndex;
                EditorSceneManager.CloseScene(EditorSceneManager.GetActiveScene(), true);
                // EditorSceneManager.CloseScene(EditorSceneManager.GetActiveScene());
                EditorSceneManager.OpenScene(scenePaths[sceneIndex]);
                SearchSelectedAsset(dataBase, scenes, assetGuid, list);
            }
            else
            {
                if (searchAssetIndex == selectAssetIndex && SceneManager.GetActiveScene().buildIndex == sceneIndex)
                    SearchSelectedAsset(dataBase, scenes, assetGuid, list);
            }

            SelectGameObject(searchAssetIndex, sceneIndex);
        }

        return selectScene;
    }

    private string GetSceneNames(string scenePath)
    {
        int cut = 0;
        for (int j = scenePath.Length - 1; j >= 0; j--)
        {
            if (scenePath[j].Equals('/'))
            {
                cut = j;
                break;
            }
        }

        string name = scenePath.Substring(cut + 1);
        return name.Substring(0, name.IndexOf("."));
    }

    private void SearchSelectedAsset(Dictionary<int, ObjectDetail[]> dataBase, int[] scenes, string assetGuid,
        List<KeyValuePair<string, int>> assetInAssets = null)
    {
        if (selectAssetIndex == -1 || selectSceneIndex == -1)
            return;

        if (prevSelectSceneIndex == selectSceneIndex && prevSelectAssetIndex == selectAssetIndex)
        {
            // Debug.Log("Repeat search in scene " + index + " for mesh " + meshNames[selectAssetIndex]);
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        int index = scene.buildIndex;
        targetGameObjects = null;
        if (!dataBase.ContainsKey(index))
        {
            // SetSearchFilter("Nothing Found!", SearchMode.FILTERMODE_NAME);
            return;
        }

        bool hasAsset = false;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i] == selectSceneIndex)
            {
                hasAsset = true;
                break;
            }
        }

        if (!hasAsset)
        {
            SetSearchFilter("", SearchMode.FILTERMODE_NAME);
            return;
        }

        List<string> targets = new List<string>();
        var objs = dataBase[index];
        if (assetInAssets == null)
        {
            // var objs = dataBase[index];         
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].assetGuid == assetGuid.GetHashCode())
                {
                    // Debug.Log("Find mesh " + meshNames[selectAssetIndex] + " in GameObject " + objs[i].name);
                    targets.Add(objs[i].name);
                }
            }
        }
        else
        {

            for (int i = 0; i < assetInAssets.Count(); i++)
            {
                int searchGuid = assetInAssets[i].Value;
                for (int j = 0; j < objs.Length; j++)
                {
                    if (objs[j].assetGuid == searchGuid.GetHashCode())
                    {
                        // Debug.Log("Find mesh " + meshNames[selectAssetIndex] + " in GameObject " + objs[i].name);
                        targets.Add(objs[i].name);
                    }
                }
            }
        }

        targetGameObjects = targets.Distinct().ToArray();
    }

    private void SelectGameObject(int assetIndex, int sceneIndex)
    {
        // Means we don't needt to find game objects attach the asset
        if (assetIndex != selectAssetIndex)
        {
            return;
        }

        // Means this is not current scene we are open
        if (SceneManager.GetActiveScene().buildIndex != sceneIndex)
        {
            return;
        }

        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = Color.green;
        if (targetGameObjects == null || targetGameObjects.Length <= 0)
        {
            style.normal.textColor = Color.red;
            EditorGUILayout.LabelField("has been attached with none game obects!!");
            return;
        }
        else
            EditorGUILayout.LabelField("has been attached with game obects: ");

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < targetGameObjects.Length; i++)
        {
            string name = targetGameObjects[i];
            if (GUILayout.Button(name, style, GUILayout.Width(10 * name.Length)))
            {
                SetSearchFilter(name, SearchMode.FILTERMODE_NAME);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // System reflection for set search filter in hierarchy  window
    private void SetSearchFilter(string filter, SearchMode filterMode)
    {
        SearchableEditorWindow hierarchy = null;
        SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
        foreach (SearchableEditorWindow window in windows)
        {

            if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
            {

                hierarchy = window;
                break;
            }
        }

        if (hierarchy == null)
        {
            Debug.Log("Can't find scene hierarchy window!!");
            return;
        }

        MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
        object[] parameters = new object[] { filter, (int)filterMode, false, false };

        setSearchType.Invoke(hierarchy, parameters);
    }
}
