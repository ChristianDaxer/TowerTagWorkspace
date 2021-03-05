using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
[CustomPropertyDrawer(typeof(PillarLightCurve))]
public class PillarLightCurveEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		// base.OnGUI(position, property, label);
		GUILayout.Label("Pillar Light Curve", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(property.FindPropertyRelative("_lightCurve"));
		EditorGUILayout.PropertyField(property.FindPropertyRelative("_speed"));
		EditorGUILayout.PropertyField(property.FindPropertyRelative("targetShader"));
		EditorGUILayout.PropertyField(property.FindPropertyRelative("searchRoot"));
		EditorGUILayout.PropertyField(property.FindPropertyRelative("cachedRenderers"));
		EditorGUILayout.PropertyField(property.FindPropertyRelative("materialIndices"));
		if (GUILayout.Button("Cache"))
			(fieldInfo.GetValue(property.serializedObject.targetObject) as PillarLightCurve).Cache();
		EditorGUILayout.Space(10);
    }
}
#endif

[ExecuteAlways]
[System.Serializable]
public class PillarLightCurve {

	[SerializeField]
	private Transform searchRoot;

	[SerializeField]private AnimationCurve _lightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField]private float _speed = 1;

	private float startTime;

	[SerializeField]
	private Shader targetShader;
	private Material[] materials;

	[SerializeField]
	private Renderer[] cachedRenderers;

	[SerializeField]
	private int[] materialCounts;

	[SerializeField]
	private int[] materialIndices;

	private float targetRange;
	private float targetIntensity;

	public void Setup ()
    {
        if (materials == null || materials.Length == 0)
        {
			if (cachedRenderers == null || cachedRenderers.Length == 0)
            {
				Debug.LogErrorFormat($"No renderers cached in {nameof(PillarLightCurve)} attached to: \"{searchRoot.name}\".");
				return;
            }

            List<Material> materialsList = new List<Material>();
			int index = 0;
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
				Material[] materials = cachedRenderers[i].materials;
				int count = materialCounts[i];
                for (int mi = index; mi < index + count; mi++)
					materialsList.Add(materials[materialIndices[mi]]);
				index += count;
            }

			materials = materialsList.ToArray();
			materialsList.Clear();

			cachedRenderers = null;
			materialCounts = null;
			materialIndices = null;
        }
    }

    public void SetLightRange (float range) 
	{
		if (materials == null || materials.Length == 0)
			return;

		targetRange = range;
		startTime = Time.time;
	}

	public void SetLightIntensity (float intensity)
    {
		if (materials == null || materials.Length == 0)
			return;

		targetIntensity = intensity;
		startTime = Time.time;
    }

	public void SetLightColor (Color color)
    {
		if (materials == null || materials.Length == 0)
			return;

        for (int i = 0; i < materials.Length; i++)
            materials[i].SetColor("_LightColor", color);
    }

    public void DisableLight()
    {
		if (materials == null || materials.Length == 0)
			return;

        for (int i = 0; i < materials.Length; i++)
            materials[i].SetFloat("_LightIntensity", 0);
    }

#if UNITY_EDITOR
    public void Cache ()
    {
		var renderers = searchRoot.GetComponentsInChildren<Renderer>();
		cachedRenderers = renderers.Where(renderer => renderer.sharedMaterials.Where(material => material != null && material.shader == targetShader).Any()).ToArray();
		List<List<int>> indices = new List<List<int>>();
		List<int> materialCountsList = new List<int>();
		cachedRenderers.ForEach(renderer =>
		{
			List<int> materialIndices = new List<int>();
			List<Material> materials = renderer.sharedMaterials.ToList();
			materialIndices.AddRange(materials.Where(material => material.shader == targetShader).Select(material => materials.IndexOf(material)));
			indices.Add(materialIndices);
			materialCountsList.Add(materialIndices.Count);
        });

		materialIndices = indices.SelectMany(a => a).ToArray();
		materialCounts = materialCountsList.ToArray();
    }
#endif

	public void UpdateLights () 
	{
		if (materials == null || materials.Length == 0)
			return;

		var time = Time.time - startTime;
		if (time  <= _speed)
		{
			var eval = _lightCurve.Evaluate(time / _speed);

			var intensity = eval * targetIntensity;
			var range = eval * targetRange;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetFloat("_LightIntensity", intensity);
                materials[i].SetFloat("_LightRange", range);
            }
		}
	}
}
