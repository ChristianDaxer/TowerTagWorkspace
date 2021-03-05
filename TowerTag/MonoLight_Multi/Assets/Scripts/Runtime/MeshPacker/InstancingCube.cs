using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancingCube : MonoBehaviour
{
    [SerializeField] private string _searchTag;
    [SerializeField] private Material _cubeMaterial;
    private MeshRenderer[] _cubesMeshRenderers;
    private Vector4[] _cubesTilings;
    private MaterialPropertyBlock _propertyBlock;
    private const string Property = "_MainTex_ST";
    private int _propertyId;

    void Start()
    {
        if (_searchTag == null)
        {
            Debug.Log("No search tag !!");
            return;
        }

        if (_cubeMaterial == null)
        {
            Debug.Log("No cubeMaterial!");
            return;
        }

        _cubeMaterial.enableInstancing = true;
       GameObject[] cubes = FindObjectwithTag(_searchTag);
        GetMainTextureProperties(cubes);
    }

    private void GetMainTextureProperties(GameObject[] _cubes)
    {
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        List<Vector4> tilings = new List<Vector4>();
        _propertyBlock = new MaterialPropertyBlock();
        _propertyId = Shader.PropertyToID(Property);

        for (int i = 0; i < _cubes.Length; i++)
        {
            MeshRenderer meshRenderer = _cubes[i].GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                renderers.Add(meshRenderer);
                Vector2 tiling = meshRenderer.sharedMaterial.mainTextureScale;
                Vector2 offSet = meshRenderer.sharedMaterial.mainTextureOffset;
                tilings.Add(new Vector4(tiling.x, tiling.y, offSet.x, offSet.y));
                meshRenderer.sharedMaterial = _cubeMaterial;
            }
        }

        _cubesMeshRenderers = renderers.ToArray();
        _cubesTilings = tilings.ToArray();
    }

    private GameObject[] FindObjectwithTag(string _tag)
    {
        Transform parent = transform;
        List<GameObject> cubes = new List<GameObject>();
        GetChildObject(parent, _tag, ref cubes);
        return cubes.ToArray();
    }

    private void GetChildObject(Transform parent, string _tag, ref List<GameObject> _gbs)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag)
            {
                _gbs.Add(child.gameObject);
            }

            if (child.childCount > 0)
            {
                GetChildObject(child, _tag, ref _gbs);
            }
        }
    }

    private void AssignPropertyBlock()
    {
        for (int i = 0; i < _cubesMeshRenderers.Length; i++)
        {
            _cubesMeshRenderers[i].GetPropertyBlock(_propertyBlock);         
            _propertyBlock.SetVector(_propertyId, _cubesTilings[i]);
            _cubesMeshRenderers[i].SetPropertyBlock(_propertyBlock);
        }
    }

    // Update is called once per frame
    void Update()
    {
        AssignPropertyBlock();      
    }
}
