using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class MeshPackerAsset : ScriptableObject
{
    public enum VertexTransformationType
    {
        PositionRotationAbsolute,
        LocalPositionZeroRotationAbsolute,
    }

    [SerializeField] private string guid;
    public string Guid => guid;

    [SerializeField] private Mesh cachedMesh;
    public Mesh CachedMesh => cachedMesh;

    [SerializeField] private Material[] materials;
    public Material[] Materials => materials; 

    [SerializeField] private int instanceCount;
    public int InstanceCount => instanceCount;

    [SerializeField] private VertexTransformationType vertexTransformationType;
    public VertexTransformationType TransformationType { get => vertexTransformationType; 
        set
        {
            vertexTransformationType = value;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    [SerializeField] private bool filterByType;
    public bool FilterByType { get { return filterByType; }
        set
        {
            filterByType = value;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    [SerializeField] private bool disableCollider = true;
    public bool DisableCollider { get { return disableCollider; }
        set
        {
            disableCollider = value;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    [SerializeField] private bool applyMeshCollider = true;
    public bool ApplyMeshCollider { get { return applyMeshCollider; }
        set
        {
            applyMeshCollider = value;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    [SerializeField] private string[] typeWhiteList = new string[0];
    public System.Type[] TypeWhiteList => typeWhiteList
        .Select(typeFullName => Type.GetType(typeFullName))
        .Where(type => type != null).ToArray();

    public void SetData (string guid, Mesh mesh, Material[] materials, int instanceCount)
    {
#if UNITY_EDITOR
        if (cachedMesh != null)
        {
            AssetDatabase.RemoveObjectFromAsset(cachedMesh);
            AssetDatabase.AddObjectToAsset(mesh, this);
        }
#endif

        this.guid = guid;
        this.cachedMesh = mesh;
        this.materials = materials;
        this.instanceCount = instanceCount;
    }

#if UNITY_EDITOR
    private static string BuildOutputFolder (string scenePath)
    {
        string outputFolderPath = Path.Combine(Path.GetDirectoryName(scenePath), "ScriptableObjects");
        if (!Directory.Exists(outputFolderPath))
            Directory.CreateDirectory(outputFolderPath);
        return outputFolderPath;
    }

    private static string BuildFilePathFromFolder (string outputFolderPath, string guid)
    {
        return Path.Combine(outputFolderPath, $"{guid}.asset");
    }

    private static MeshPackerAsset CreateAsset (string outputFolder, string guid)
    {
        MeshPackerAsset asset = MeshPackerAsset.CreateInstance<MeshPackerAsset>();
        asset.guid = guid;

        string fullOutputPath = BuildFilePathFromFolder(outputFolder, guid);
        AssetDatabase.CreateAsset(asset, fullOutputPath);
        return asset;
    }

    private static MeshPackerAsset LoadAsset (string outputFolder, string guid)
    {
        return AssetDatabase.LoadAssetAtPath<MeshPackerAsset>(BuildFilePathFromFolder(outputFolder, guid));
    }

    private struct Data
    {
        public Transform objectTransform;
        public int objectIndex;
        public int subMeshIndex;
        public SubMeshDescriptor subMeshDescriptor;
    }

    public static System.Type[] ListPotentialTargets (GameObject selected)
    {
        return selected
            .GetComponentsInChildren<MonoBehaviour>()
            .Concat(selected.GetComponents<MonoBehaviour>())
            .Where(monoBehaviour => monoBehaviour.GetComponent<MeshFilter>() != null && monoBehaviour.GetComponent<MeshRenderer>())
            .Select(monoBehaviour => monoBehaviour.GetType())
            .Distinct().ToArray();
    }

    public bool[] BuildTypeMask (System.Type[] types)
    {
        return types.Select(type => typeWhiteList.Contains(type.FullName)).ToArray();
    }

    public void SetTypeWhiteList (System.Type[] typeWhiteList)
    {
        var typeFullNameWhiteList = typeWhiteList.Select(type => type.FullName);

        string msg = $"Setting {nameof(PackedMesh)} type white list to:";
        typeFullNameWhiteList.ForEach(typeFullName => msg += $"\n\t{typeFullName}");
        Debug.Log(msg);

        this.typeWhiteList = typeFullNameWhiteList.ToArray();

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public static void Pack (GameObject selected, bool newPacker = false)
    {
        var packedMeshes = selected.GetComponents<PackedMesh>();
        if (newPacker || packedMeshes.Length == 0)
            packedMeshes = new PackedMesh[1] { selected.AddComponent<PackedMesh>() };

        foreach (var packedMesh in packedMeshes)
        {
            string outputFolder = BuildOutputFolder(selected.scene.path);
            if (string.IsNullOrEmpty(packedMesh.Guid))
            {
                string guid = GUID.Generate().ToString();
                packedMesh.SetData(guid, CreateAsset(outputFolder, guid));
            }

            else if (packedMesh.Asset == null)
            {
                var asset = LoadAsset(outputFolder, packedMesh.Guid);
                if (asset == null)
                    asset = CreateAsset(outputFolder, packedMesh.Guid);
                packedMesh.SetData(packedMesh.Guid, asset);
            }

            IEnumerable<MeshFilter> meshFilters = null;
            if (packedMesh.Asset.FilterByType)
            {
                meshFilters = selected.GetComponentsInChildren<MeshFilter>()
                    .Where(meshFilter => 
                        meshFilter.gameObject.activeInHierarchy &&
                        meshFilter.sharedMesh != null && 
                        meshFilter.GetComponent<MeshRenderer>() != null &&
                        packedMesh.Asset.TypeWhiteList.Any(type => meshFilter.GetComponent(type) != null));
            }

            else meshFilters = selected.GetComponentsInChildren<MeshFilter>()
                .Where(meshFilter => 
                meshFilter.gameObject.activeInHierarchy &&
                meshFilter.sharedMesh != null && 
                meshFilter.GetComponent<MeshRenderer>() != null);

            var monoBehaviours = selected.GetComponentsInChildren<MonoBehaviour>().Concat(selected.GetComponents<MonoBehaviour>()).Distinct();

            var indexedMeshes = monoBehaviours
                    .Where(monoBehaviour => monoBehaviour is IIndexedMesh)
                    .Select(monoBehaviour => monoBehaviour as IIndexedMesh);

            var multiIndices = monoBehaviours
                    .Where(monoBehaviour => monoBehaviour is IMultiIndexedMesh)
                    .Select(monoBehaviour => monoBehaviour as IMultiIndexedMesh);

            multiIndices.ForEach(multiIndex => multiIndex.ResetMeshIndices());

            List<Material> materials = new List<Material>();
            GameObject gameObject = new GameObject($"{selected.name}-Packed");
            gameObject.isStatic = true;

            MeshFilter outputMeshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer outputMeshRenderer = gameObject.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = null;
            if (packedMesh.Asset.ApplyMeshCollider)
                meshCollider = gameObject.AddComponent<MeshCollider>();
            Mesh outputMesh = new Mesh();

            Dictionary<Material, Dictionary<Mesh, List<Data>>> materialToSubmeshesLut = new Dictionary<Material, Dictionary<Mesh, List<Data>>>();

            for (int mi = 0; mi < meshFilters.Count(); mi++)
            {
                MeshFilter meshFilter = meshFilters.ElementAt(mi);
                Mesh meshInstance = meshFilter.sharedMesh;

                indexedMeshes
                    .ForEach(indexedMesh =>
                    {
                        if (indexedMesh.GameObjectInstanceID != meshFilter.gameObject.GetInstanceID())
                            return;

                        indexedMesh.MeshIndex = mi;
                        EditorUtility.SetDirty((MonoBehaviour)indexedMesh);
                    });

                Transform parent = meshFilter.transform.parent;
                while (parent != null)
                {
                    multiIndices
                        .Where(indexedMesh => indexedMesh.GameObjectInstanceID == parent.gameObject.GetInstanceID())
                        .ForEach(indexedMesh =>
                        {
                            indexedMesh.AppendMeshIndex(mi);
                            EditorUtility.SetDirty((MonoBehaviour)indexedMesh);
                        });

                    parent = parent.parent;
                }

                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

                Material[] sharedMaterials = meshRenderer.sharedMaterials;
                for (int si = 0; si < meshInstance.subMeshCount; si++)
                {
                    if (sharedMaterials[si] == null)
                        continue;

                    SubMeshDescriptor subMeshDescriptor = meshInstance.GetSubMesh(si);

                    Data newData = new Data
                    {
                        objectTransform = meshFilter.transform,
                        objectIndex = mi,
                        subMeshIndex = si,
                        subMeshDescriptor = subMeshDescriptor
                    };

                    if (!materialToSubmeshesLut.TryGetValue(sharedMaterials[si], out var data))
                        materialToSubmeshesLut.Add(sharedMaterials[si], new Dictionary<Mesh, List<Data>>() { { meshInstance, new List<Data>() { newData } } });
                    else if (!data.ContainsKey(meshInstance))
                        data.Add(meshInstance, new List<Data>() { newData });
                    else data[meshInstance].Add(newData);
                }
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs0 = new List<Vector2>();
            List<Vector2> uvs1 = new List<Vector2>();
            List<List<int>> indices = new List<List<int>>();

            float lutSideLength = Mathf.Ceil(Mathf.Sqrt(meshFilters.Count()));

            foreach (var materialAndSubmeshes in materialToSubmeshesLut)
            {
                Dictionary<Mesh, List<Data>> submeshes = materialAndSubmeshes.Value;
                List<Vector3> submeshVertices = new List<Vector3>();
                List<Vector3> submeshNormals = new List<Vector3>();
                List<Vector2> submeshUVs = new List<Vector2>();
                List<int> submeshIndices = new List<int>();

                foreach (var mesh in submeshes)
                {
                    mesh.Key.GetVertices(submeshVertices);
                    mesh.Key.GetNormals(submeshNormals);
                    mesh.Key.GetUVs(0, submeshUVs);

                    foreach (var data in mesh.Value)
                    {
                        SubMeshDescriptor subMeshDescriptor = data.subMeshDescriptor;

                        var instanceVertices = submeshVertices
                            .GetRange(subMeshDescriptor.firstVertex, subMeshDescriptor.vertexCount)
                            .Select(vertex =>
                            {
                                switch (packedMesh.Asset.TransformationType)
                                {
                                    case VertexTransformationType.PositionRotationAbsolute:
                                    default:
                                        return data.objectTransform.localToWorldMatrix.MultiplyPoint(vertex);

                                    case VertexTransformationType.LocalPositionZeroRotationAbsolute:
                                        Matrix4x4 localToWorld = data.objectTransform.localToWorldMatrix;
                                        if (data.objectTransform.parent != null)
                                            localToWorld.SetColumn(3, data.objectTransform.parent.localToWorldMatrix.GetColumn(3));
                                        return localToWorld.MultiplyPoint(vertex);
                                }
                            });

                        var instanceIndices = mesh.Key.GetIndices(data.subMeshIndex).Select(index => index - data.subMeshDescriptor.firstVertex + vertices.Count);
                        submeshIndices.AddRange(instanceIndices);

                        vertices.AddRange(instanceVertices);

                        normals.AddRange(submeshNormals
                            .GetRange(subMeshDescriptor.firstVertex, subMeshDescriptor.vertexCount)
                            .Select(normal => data.objectTransform.localToWorldMatrix.MultiplyVector(normal)));

                        var instanceUVs = submeshUVs.GetRange(subMeshDescriptor.firstVertex, subMeshDescriptor.vertexCount);
                        uvs0.AddRange(instanceUVs);
                        uvs1.AddRange(instanceUVs.Select(uv =>
                        {
                            return new Vector2(
                                (data.objectIndex % lutSideLength) / lutSideLength,
                                Mathf.Floor(data.objectIndex / lutSideLength) / lutSideLength);
                        }));
                    }
                }

                indices.Add(submeshIndices);
            }

            { // Setup the mesh.
                outputMesh.name = selected.name;
                outputMesh.vertices = vertices.ToArray();
                outputMesh.normals = normals.ToArray();
                outputMesh.SetUVs(0, uvs0);
                outputMesh.SetUVs(2, uvs1);

                outputMesh.subMeshCount = indices.Count;
                for (int i = 0; i < indices.Count; i++)
                    outputMesh.SetIndices(indices[i].ToArray(), MeshTopology.Triangles, i);

                outputMeshFilter.sharedMesh = outputMesh;
                outputMeshRenderer.sharedMaterials = materialToSubmeshesLut.Keys.ToArray();
                if (meshCollider != null)
                    meshCollider.sharedMesh = outputMesh;

                SceneManager.MoveGameObjectToScene(outputMeshRenderer.gameObject, selected.scene);
            }

            packedMesh.Asset.SetData(packedMesh.Guid, outputMesh, materialToSubmeshesLut.Keys.ToArray(), meshFilters.Count());
            packedMesh.SetTargetMeshRenderer(outputMeshRenderer);

            EditorUtility.SetDirty(packedMesh.Asset);
            AssetDatabase.Refresh();

            packedMesh.ToggleUnpackedRenderers(false);
        }
    }

    public static void Pack (bool newPacker = false)
    {
        GameObject selected = Selection.activeGameObject as GameObject;
        if (selected == null && selected.scene != null)
            return;
        Pack(selected, newPacker);
    }
#endif
}
