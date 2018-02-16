using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using nWayPoint;

[CustomEditor(typeof(SimpleAgent))]
public class SimpleAgentEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		SimpleAgent agent = this.target as SimpleAgent;
		base.OnInspectorGUI ();
		if(agent.waypoint != null)
		{
			if(GUILayout.Button("Update Position"))
			{
				agent.transform.position = agent.waypoint.GetPositionOnTrail (agent.factor);
			}
		}
	}
}
