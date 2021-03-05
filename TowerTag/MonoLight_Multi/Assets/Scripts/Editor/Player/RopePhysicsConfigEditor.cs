using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Rope { 
	[CustomEditor(typeof (RopePhysicsConfig ))]
	public class RopePhysicsConfigEditor : Editor {
		public override void OnInspectorGUI()
		{
			
			DrawDefaultInspector();
			// trigger Update on interplator 
			var intp  = Component.FindObjectOfType( typeof ( InterpolatedConfig )) as InterpolatedConfig;
            if ( intp) intp.T = intp.T ; 

		}
		
	}
}