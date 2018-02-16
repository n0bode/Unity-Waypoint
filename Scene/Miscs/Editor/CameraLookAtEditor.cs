using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraLookAt))]
public class CameraLookAtEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		CameraLookAt self = this.target as CameraLookAt;
		base.OnInspectorGUI ();

		if(self.target != null)
		{
			if(GUILayout.Button("Look At Target"))
			{
				self.LookAt ();
			}
		}
	}
}
