using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MeshDebugger : MonoBehaviour
{
    [SerializeField] private bool _showVertexIndices;
    [SerializeField] private bool _showVertexPositions;
    [SerializeField] private bool _showVertexNormals;
    [SerializeField] private bool _showUVs;
    [SerializeField] private bool _showTriangleNumbers;
    [SerializeField] private bool _showTriangleVertexIndices;
    [SerializeField] private bool _showTriangleNormals;
    [SerializeField] private bool _showTangents;
    [SerializeField] private bool _showTriangleGizmo;
    [SerializeField] private float _lengthFactor = 0.5f;
    [SerializeField] private float _fontSizeFactor = 1f;

    #region The Mesh

    private MeshFilter _meshFilter;

    // The mesh
    private Mesh _mesh;
    #endregion // The Mesh

    #region VertexCulling
    // Ray for VertexCulling
    private Ray _ray;
    private RaycastHit _hit;

    private Vector3 _cameraPosition;
    private static Vector3 CameraPosition => Camera.current.transform.position;

    // used for culling
    private readonly Dictionary<int, bool> _vertexVisible = new Dictionary<int, bool>();
    #endregion // VertexCulling

    #region GUIStyle for labels
    private GUIStyle _vertexLabelStyle;
    private int FontSize { get; } = 20;

    #endregion // GUIStyle for labels

    [SerializeField] private Vector3 _eulerAngles = new Vector3(0, 1f, 0);
    [SerializeField] private bool _rotate;
    private Transform _target;

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        _target = transform;
    }

    private void EditorUpdate()
    {
        if (_rotate)
        {
            _target.rotation *= Quaternion.Euler(_eulerAngles);
        }
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    // Use this for initialization
    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter != null)
            _mesh = _meshFilter.sharedMesh;

        _vertexLabelStyle = new GUIStyle {richText = true};
        CreateMaterial();
    }

    private void OnDrawGizmos()
    {
        CreateMaterial();
        if (_mesh != null)
        {
            CheckVertexVisibility();

            if (_showVertexPositions)
                ShowVertexPositions();

            if (_showVertexIndices)
                ShowVertexIndices();

            if (_showVertexNormals)
                ShowVertexNormals();

            if (_showUVs)
                ShowUVs();

            if (_showTriangleNumbers)
                ShowTriangleNumbers();

            if (_showTriangleNormals)
                ShowTriangleNormals();

            if (_showTriangleVertexIndices)
                ShowTriangleVertexIndices();

            if (_showTangents)
                ShowTangents();

            if (_showTriangleGizmo)
                ShowTriangles();
        }
        else
        {
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = _meshFilter.sharedMesh;

            _vertexLabelStyle = new GUIStyle {richText = true};
        }
    }

    /// <summary>
    /// Check if a Position is visible (sight not blocked by the mesh)
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool PositionVisible(Vector3 pos)
    {
        if (Physics.Raycast(CameraPosition, pos - CameraPosition, out _hit, 100f))
        {
            if (Vector3.Distance(_hit.point, pos) < 0.02f)
                return true;
            return false;
        }
        return true;
    }

    private Vector3 CorrectToTransform(Vector3 point) {
        Transform thisTransform = transform;
        Vector3 position = thisTransform.position;
        return RotatePointAroundPivot(point+position, position, thisTransform.rotation.eulerAngles);
    }

    private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        // get point direction relative to pivot
        Vector3 dir = point - pivot;
        // rotate it
        dir = Quaternion.Euler(angles) * dir;
        // calculate rotated point
        point = dir + pivot;
        return point;
    }

    /// <summary>
    /// Check all vertex Positions if they are visible to the Scene-View camera
    /// </summary>
    private void CheckVertexVisibility()
    {
        for (var i = 0; i < _mesh.vertexCount; i++)
            _vertexVisible[i] = PositionVisible(CorrectToTransform(_mesh.vertices[i]));
    }

    private void CenteredLabel(Vector3 worldPosition, string label, float fontSize, Color color)
    {
        string colorString = $"#{(int) (255f * color.r):x02}{(int) (255f * color.g):x02}{(int) (255f * color.b):x02}";
        string text = "<size=" + fontSize * _fontSizeFactor + "><color=" + colorString + ">" + label + "</color></size>";

        var content = new GUIContent(text);

        _vertexLabelStyle.CalcMinMaxWidth(content, out float _, out float max);
        float height = _vertexLabelStyle.CalcHeight(content, max);

        Vector2 pos2D = HandleUtility.WorldToGUIPoint(worldPosition);  
        GUI.Label(new Rect(pos2D.x - max / 2f, pos2D.y - height , max, height), text, _vertexLabelStyle);
    }

    #region Vertex-Informations
    private void ShowVertexIndices()
    {
        Handles.BeginGUI();
        for (var i = 0; i < _mesh.vertexCount; i++)
        {
            Vector3 pos = CorrectToTransform(_mesh.vertices[i]);
            if (_vertexVisible[i])
            {
                CenteredLabel(pos, i.ToString(), FontSize, Color.red);
            }
        }
        Handles.EndGUI();
    }

    private void ShowVertexPositions()
    {
        Handles.BeginGUI();
        for (var i = 0; i < _mesh.vertexCount; i++)
        {
            Vector3 pos = CorrectToTransform(_mesh.vertices[i]);

            if (_vertexVisible[i])
            {
                CenteredLabel(pos, _mesh.vertices[i].ToString(), FontSize, Color.red);
            }
        }
        Handles.EndGUI();
    }

    private void ShowVertexNormals()
    {
        Handles.color = Color.red;
        for (var i = 0; i < _mesh.normals.Length; i++)
        {
            if (_vertexVisible[i])
            {
                Vector3 vertexPosition = CorrectToTransform(_mesh.vertices[i]);
                Vector3 normal = _mesh.normals[i] * _lengthFactor;
                Handles.color = new Color(Mathf.Abs(normal.x + 0.5f), Mathf.Abs(normal.y + 0.5f), Mathf.Abs(normal.z + 0.5f));
                Handles.DrawLine(vertexPosition, vertexPosition + normal);
            }
        }
    }

    private void ShowUVs()
    {
        Handles.BeginGUI();
        for (var i = 0; i < _mesh.uv.Length; i++)
        {
            Vector3 pos = CorrectToTransform(_mesh.vertices[i]);
            if (_vertexVisible[i])
                CenteredLabel(pos, _mesh.uv[i].ToString(), FontSize, Color.red);
        }
        Handles.EndGUI();
    }
    #endregion // Vertex-Informations

    /// <summary>
    /// Show the Vertex-Indices of the triangles on the triangle-surfaces
    /// </summary>
    private void ShowTriangleVertexIndices()
    {
        Handles.BeginGUI();
        for (var i = 0; i < _mesh.triangles.Length; i += 3)
        {
            if (_vertexVisible[_mesh.triangles[i]] && _vertexVisible[_mesh.triangles[i + 1]] && _vertexVisible[_mesh.triangles[i + 2]])
            {
                Vector3 p0 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i]]);
                Vector3 p1 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 1]]);
                Vector3 p2 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 2]]);

                Vector3 centroid = (p0 + p1 + p2) / 3f;

                if (PositionVisible(centroid))
                {
                    Vector3 p0Label = centroid + (p0 - centroid) * 0.7f;
                    CenteredLabel(p0Label, _mesh.triangles[i].ToString(), FontSize, Color.blue);

                    Vector3 p1Label = centroid + (p1 - centroid) * 0.7f;
                    CenteredLabel(p1Label, _mesh.triangles[i + 1].ToString(), FontSize, Color.blue);

                    Vector3 p2Label = centroid + (p2 - centroid) * 0.7f;
                    CenteredLabel(p2Label, _mesh.triangles[i + 2].ToString(), FontSize, Color.blue);
                }
            }
        }
        Handles.EndGUI();
    }

    private void ShowTriangleNumbers()
    {
        Handles.BeginGUI();
        for (var i = 0; i < _mesh.triangles.Length; i += 3)
        {
            if (_vertexVisible[_mesh.triangles[i]] && _vertexVisible[_mesh.triangles[i + 1]] && _vertexVisible[_mesh.triangles[i + 2]])
            {
                Vector3 p0 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i]]);
                Vector3 p1 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 1]]);
                Vector3 p2 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 2]]);

                Vector3 centroid = (p0 + p1 + p2) / 3f;
                if (PositionVisible(centroid))
                    CenteredLabel(centroid, (i / 3).ToString(), FontSize*2, new Color(1f, 0.6f, 0f));
            }
        }
        Handles.EndGUI();
    }

    private void ShowTriangleNormals()
    {
        Handles.color = Color.yellow;
        for (var i = 0; i < _mesh.triangles.Length; i += 3)
        {
            if (_vertexVisible[_mesh.triangles[i]] && _vertexVisible[_mesh.triangles[i + 1]] && _vertexVisible[_mesh.triangles[i + 2]])
            {
                Vector3 p0 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i]]);
                Vector3 p1 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 1]]);
                Vector3 p2 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 2]]);

                Vector3 centroid = (p0 + p1 + p2) / 3f;
                Vector3 normal = Normal(p0, p1, p2).normalized *_lengthFactor;

                Handles.color = new Color(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
                Handles.DrawLine(centroid, centroid + normal);
            }
        }
    }

    private void ShowTangents()
    {
        for (var i = 0; i < _mesh.tangents.Length; i += 3)
        {
            //if (vertexVisible[i])
            //{
            Vector4 tangent = _mesh.tangents[i];
            Vector3 tangent3 = tangent.normalized * _lengthFactor;
            Handles.color = new Color(Mathf.Abs(tangent.x), Mathf.Abs(tangent.y), Mathf.Abs(tangent.z));
            Handles.DrawLine(CorrectToTransform(_mesh.vertices[i]), CorrectToTransform((_mesh.vertices[i] + tangent3 * tangent.w)));
            //}
        }
    }


    private void ShowTriangles()
    {
        for (var i = 0; i < _mesh.triangles.Length; i += 3)
        {
            if (_vertexVisible[_mesh.triangles[i]] && _vertexVisible[_mesh.triangles[i + 1]] && _vertexVisible[_mesh.triangles[i + 2]])
            {
                Vector3 p0 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i]]);
                Vector3 p1 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 1]]);
                Vector3 p2 = CorrectToTransform(_mesh.vertices[_mesh.triangles[i + 2]]);

                Vector3 normal = Normal(p0, p1, p2).normalized;

                DrawTriangleMeshSimple(p0, p1, p2, new Color(Mathf.Abs(normal.x + 0.5f), Mathf.Abs(normal.y + 0.5f), Mathf.Abs(normal.z + 0.5f)));
            }
        }
    }

    private static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Vector3.Cross((p1 - p0), (p2 - p0));
    }

    #region TriangleGizmo

    private static void DrawTriangleMeshSimple(Vector3 p0, Vector3 p1, Vector3 p2, Color color)
    {
        color.a = Mathf.Clamp(color.a, 0f, 0.8f);
        GL.PushMatrix();
        _material.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);
        GL.Vertex3(p0.x, p0.y, p0.z);
        GL.Color(color);
        GL.Vertex3(p1.x, p1.y, p1.z);
        GL.Color(color);
        GL.Vertex3(p2.x, p2.y, p2.z);
        GL.End();
        GL.PopMatrix();
    }

    private static Material _material;
    private static void CreateMaterial()
    {
        if (!_material)
        {
#pragma warning disable 618
            _material = new Material("Shader \"Lines/Colored Blended\" {" +
#pragma warning restore 618
                                     "SubShader { Pass { " +
                                     " Blend SrcAlpha OneMinusSrcAlpha " +
                                     " ZWrite Off Cull Off Fog { Mode Off } " +
                                     " BindChannels {" +
                                     " Bind \"vertex\", vertex Bind \"color\", color }" +
                                     "} } }") {
                hideFlags = HideFlags.HideAndDontSave, shader = {hideFlags = HideFlags.HideAndDontSave}
            };

        }
    }
    #endregion // TriangleGizmo
}