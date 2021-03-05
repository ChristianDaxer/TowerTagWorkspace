using UnityEngine;
using System.Collections;
using UnityEditor;



namespace Rope {
	[CustomEditor(typeof(InterpolatedConfig))]
	public class InterpolatedConfigEditor : Editor {
		public override void OnInspectorGUI()
		{
			InterpolatedConfig T = target as InterpolatedConfig;
			T.T = T.T ; // trigger interpolate factor update 
			DrawDefaultInspector();
		}
	}
}	