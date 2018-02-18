/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WayPoint;

namespace WayPointEditor
{
	[CustomEditor(typeof(WaypointAgent)), CanEditMultipleObjects]
	public class WaypointAgentEditor : Editor
	{
		private WaypointAgent self;

		void OnEnable()
		{
			this.self = target as WaypointAgent;
			Undo.undoRedoPerformed += this.OnUndoRedo;
		}

		void OnUndoRedo()
		{
			if(self.manager != null)
			{
				self.UpdatePosition();
			}
		}

		public override void OnInspectorGUI ()
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.Space(5);

			WaypointManager manager = EditorGUILayout.ObjectField("Waypoint Manager", self.manager, typeof(WaypointManager), true) as WaypointManager;
			float factor = EditorGUILayout.Slider("Factor", self.factor, -1f, 1f);
			float speed = EditorGUILayout.FloatField("Speed", self.speed);
			float height = EditorGUILayout.FloatField("Height", self.height);
			float baseOffset = EditorGUILayout.FloatField("BaseOffset", self.baseOffset);
			float radius = EditorGUILayout.FloatField("Radius", self.radius);
			bool completeTrail = EditorGUILayout.Toggle("Complete Trail", self.completeTrail);
			bool loop = EditorGUILayout.Toggle("Loop", self.loop);

			if(self.manager != null)
			{
				if(Application.isPlaying)
				{
					if(GUILayout.Button(self.isStopped ? "Return" : "Stop"))
					{
						self.isStopped = !self.isStopped;
					}
				}
				else
				{
					if(GUILayout.Button("Update Position"))
					{
						self.SetPosition(self.manager.GetPositionOnTrail(self.factor));
					}
				}
			}
			else
			{
				GUILayout.Label("It needs a WaypointManager to move", new GUIStyle("Helpbox"));
			}

			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(self, "Change Agent");
				self.manager = manager;
				self.factor = factor;
				self.speed = speed;
				self.height = height;
				self.baseOffset = baseOffset;
				self.radius = radius;
				self.completeTrail = completeTrail;
				self.loop = loop;
				self.UpdatePosition();
			}
		}
	}
}
