/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1
/// Github Page: https://github.com/n0bode/Unity-Waypoint

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using WayPoint;

namespace WayPointEditor
{
	[CustomEditor(typeof(WaypointManager))]
	public class WaypointManagerEditor : Editor
	{
		[System.Flags]
		enum RTool : short
		{
			VIEW = 0x00,
			MOVE = 0x02,
			BEZIER = 0x04,
			SETTINGS = 0x08
		}

		private ReorderableList m_relist;
		private WaypointManager m_waypoint;
		private RTool m_tool;
		private RTool m_lastTool;

		private bool m_editable;
		[SerializeField]
		private int m_index = -1;

		#region MonoBehaviour Callbacks

		void OnEnable()
		{
			//Show Unity's Controls
			Tools.hidden = false;
			//Parse Target to WaypointManager
			this.m_waypoint = (WaypointManager)this.target;
			if(this.m_waypoint.waypointData != null)
			{
				this.BuildList();
				//Hide Gizmos Trail
				this.m_waypoint.drawTrail = false;
			}
			//When Undo or Redo were called, it calls this methods
			Undo.undoRedoPerformed += this.OnUndoRedo;
		}

		void OnDisable()
		{
			//Hide Unity's Controls
			Tools.hidden = false;
			//Draw Gizmos Trail
			this.m_waypoint.drawTrail = true;
			Undo.undoRedoPerformed -= this.OnUndoRedo;
		}

		void OnUndoRedo()
		{
			//Rebuild list
			if(this.m_relist != null)
			{
				this.m_relist.index = this.m_index;
				this.Repaint ();
				this.BuildList ();
			}
		}
		#endregion

		#region ReorderList Callbacks
		//Callback Header of ReorderableList
		void ListDrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, new GUIContent("Points"));
		}

		//Callback Elements of ReorderableList
		void ListElementDraw(Rect rect, int index, bool active, bool focused)
		{
			Event evt = Event.current;
			Point point = this.m_waypoint.waypointData[index];
			GUI.Label(new Rect( rect.x, rect.y, 50, rect.height), point.id.ToString());
			if(this.m_editable && this.m_index == index)
			{
				this.m_waypoint.waypointData[index].name = EditorGUI.TextField(new Rect(rect.x + 50, rect.y + 1, rect.width - 50, rect.height - 4), point.name);
			}
			else
			{
				GUI.Label(new Rect(rect.x + 50, rect.y + 1, rect.width - 50, rect.height - 4), point.name, new GUIStyle("label"));
			}

			if(rect.Contains(evt.mousePosition) && evt.isMouse)
			{
				if(evt.button == 0 && evt.clickCount == 2)
				{
					this.m_editable = true;
					this.Repaint();
				}
			}

			if(evt.isKey && evt.keyCode == KeyCode.Return)
			{
				this.m_editable = false;
				this.Repaint();
			}
		}

		//Callback when a element is added to List
		void OnAddPoint(ReorderableList list)
		{
			Undo.RecordObjects (new Object[]{this.m_waypoint.waypointData, this}, "Duplicate Point");
			if(this.m_waypoint.waypointData.length < 1)
			{
				this.m_waypoint.waypointData.AddPoint(new Point()).name = "Point";
			}
			else
			{
				this.m_waypoint.waypointData.Duplicate(this.m_index);
			}
			this.m_index++;
			this.BuildList();
			this.Repaint();
		}

		//Callback when a element is removed from list
		void OnRemovePoint(ReorderableList list)
		{
			//Record Changes to Undo
			Undo.RecordObjects (new Object[]{this.m_waypoint.waypointData, this}, "Remove Point");
			this.m_waypoint.waypointData.RemovePoint(this.m_waypoint.waypointData[list.index]);
			this.m_index = (this.m_waypoint.waypointData.length == 0) ? -1 : ((this.m_index > 0) ? this.m_index - 1 : 0);
			this.BuildList();
			this.Repaint();
		}

		//Callback when a element is selected
		void OnSelectList(ReorderableList list)
		{
			if(this.m_index == list.index)
				return;

			this.m_index = list.index;
			this.m_editable = false;
			Tools.hidden = true;
			if(this.m_tool == RTool.VIEW || this.m_tool == RTool.SETTINGS)
				this.m_tool = RTool.MOVE;
		}
		#endregion

		#region InspectorGUI Methods
		//Build ReorderableList
		void BuildList ()
		{
			if (this.m_waypoint.waypointData != null)
			{
				this.m_relist = new ReorderableList (this.m_waypoint.waypointData.m_points, typeof(Point));
				this.m_relist.drawHeaderCallback = this.ListDrawHeader;
				this.m_relist.onAddCallback = this.OnAddPoint;
				this.m_relist.onRemoveCallback = this.OnRemovePoint;
				this.m_relist.drawElementCallback = this.ListElementDraw;
				this.m_relist.onSelectCallback = this.OnSelectList;
				this.m_editable = false;
				this.m_relist.index = this.m_index;
			}
		}

		//Just to do a ToolBar with an Enum Flag
		bool ButtonToggle(bool value, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
		{
			return GUILayout.Toggle(value, content, style, options) != value;
		}

		//Show the Control Panel of Tools
		void DrawToolControls()
		{
			GUILayout.BeginHorizontal();
			if(this.ButtonToggle(this.m_tool == RTool.VIEW, EditorGUIUtility.IconContent("ViewToolOrbit"), new GUIStyle("miniButtonLeft"), GUILayout.Height(20)))
			{
				this.m_lastTool = m_tool;
				this.m_tool = RTool.VIEW;
				Tools.hidden = false;
			}

			if(this.ButtonToggle((this.m_tool & RTool.MOVE) == RTool.MOVE, EditorGUIUtility.IconContent("MoveTool"), new GUIStyle("miniButtonMid"), GUILayout.Height(20)))
			{
				if ((this.m_tool & RTool.MOVE) == RTool.MOVE)
					this.m_tool &= RTool.MOVE;
				else
					this.m_tool |= RTool.MOVE;
				Tools.hidden = true;
			}

			//PS:
			//I did not a Icon like look a curve bezier, so i had using this dot icon as icon for bezier
			//If you found a icon like look bezier, send a message or modify this script
			if(this.ButtonToggle((this.m_tool & RTool.BEZIER) == RTool.BEZIER, EditorGUIUtility.IconContent("sv_icon_dot8_sml"), new GUIStyle("miniButtonMid"), GUILayout.Height(20)))
			{
				if ((this.m_tool & RTool.BEZIER) == RTool.BEZIER)
					this.m_tool &= RTool.BEZIER;
				else
					this.m_tool |= RTool.BEZIER;
				Tools.hidden = true;
			}

			if(this.ButtonToggle(this.m_tool == RTool.SETTINGS, EditorGUIUtility.IconContent("SettingsIcon"), new GUIStyle("miniButtonRight"), GUILayout.Height(20)))
			{
				this.m_lastTool = m_tool;
				this.m_tool = RTool.SETTINGS;
				Tools.hidden = false;
			}
			GUILayout.EndHorizontal();
			this.Separator();
		}

		//Only Vector3Field with a button to reset Values
		Vector3 Vector3Field(string name, Vector3 value)
		{
			GUILayout.BeginHorizontal ();
			value = EditorGUILayout.Vector3Field (name, value);
			if(GUILayout.Button("☓", GUILayout.Width(18)))
			{
				value = Vector3.zero;
			}
			GUILayout.EndHorizontal ();
			return value;
		}

		//Drawing Infos of Point
		void DrawInfoPoint()
		{
			//Case if some point is selected
			if(this.m_index >= 0)
			{
				GUILayout.BeginVertical(new GUIStyle("box"));
				Point point = this.m_waypoint.waypointData[this.m_index];
				point.position = this.Vector3Field("Position", point.position);
				if(point.uniqueTangent)
				{
					point.tangent = this.Vector3Field("Tangent", point.tangent);
				}
				else
				{
					point.tangentL = this.Vector3Field("TangentL", point.tangentL);
					point.tangentR = this.Vector3Field("TangentR", point.tangentR);
				}
				GUILayout.BeginHorizontal (new GUIStyle("box"));
				point.uniqueTangent = GUILayout.Toggle (point.uniqueTangent, "Unique Tangent");
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical();
				this.Separator();
				this.m_waypoint.waypointData[this.m_index] = point;
			}
		}

		//Show the Settings in Interface
		private void DrawSettings()
		{
			GUILayout.BeginVertical (GUI.skin.box);
			this.m_waypoint.trailColor = EditorGUILayout.ColorField ("Trail Color", this.m_waypoint.trailColor);
			this.m_waypoint.pointColor = EditorGUILayout.ColorField ("Point Color", this.m_waypoint.pointColor);
			this.m_waypoint.selectedPointColor = EditorGUILayout.ColorField ("Selected Point Color", this.m_waypoint.selectedPointColor);
			this.m_waypoint.drawTrailGizmos = EditorGUILayout.Toggle ("Draw Trail Gizmos", this.m_waypoint.drawTrailGizmos);
			this.m_waypoint.completeTrail = EditorGUILayout.Toggle ("Complete Trail", this.m_waypoint.completeTrail);
			this.m_waypoint.drawTrailAA = EditorGUILayout.Toggle ("Draw TrailAA", this.m_waypoint.drawTrailAA);
			this.m_waypoint.trailAAWidth = EditorGUILayout.IntField ("TrailAA Width", this.m_waypoint.trailAAWidth);
			this.m_waypoint.trailDetailLevel = EditorGUILayout.IntSlider ("Trail Detail Level", this.m_waypoint.trailDetailLevel, 0, 100);
			GUILayout.EndVertical ();
		}

		//A Sample Separator
		private void Separator()
		{
			GUILayout.Box("", new GUIStyle("IN Title"), GUILayout.ExpandWidth(true), GUILayout.Height(1));
		}

		public override void OnInspectorGUI ()
		{
			Event evt = Event.current;
			EditorGUILayout.Space ();
			this.m_waypoint.waypointData = (WaypointData)EditorGUILayout.ObjectField ("WayPointData", this.m_waypoint.waypointData, typeof(WaypointData), true);
			EditorGUILayout.Space ();
			if(this.m_waypoint.waypointData != null)
			{
				if(this.m_relist == null)
					this.BuildList();

				GUILayout.BeginVertical(new GUIStyle("helpbox"));
				this.DrawToolControls();
				if(this.m_tool != RTool.SETTINGS)
				{
					this.DrawInfoPoint();
					this.m_relist.DoLayoutList();
				}
				else
				{
					this.DrawSettings ();
				}
				GUILayout.Space(5);
				GUILayout.EndVertical();
				//This is important to save change of WaypointData to Asset
				EditorUtility.SetDirty (this.m_waypoint.waypointData);

				//Modifying Unity Commands to this Code
				if(evt.type == EventType.ValidateCommand)
				{
					//Here duplicate selected point in List
					if(evt.commandName == "Duplicate")
					{
						this.OnAddPoint (this.m_relist);
						evt.Use ();
					}
					//Here delete selected point in List
					else if(evt.commandName == "SoftDelete")
					{
						this.OnRemovePoint (this.m_relist);
						evt.Use ();
					}
				}
			}
			else
			{
				GUILayout.TextArea ("You need to create a WaypointData!\n To do that, just you go Assets->Create->WayPointData", new GUIStyle("helpbox"));
			}

		}
		#endregion

		#region HANDLES
		//Draw Point of Waypoint
		Point HandleDrawPoint(int index, Point point, Vector3 cameraPos, bool selected)
		{
			Event evt = Event.current;
			Handles.color = selected ? this.m_waypoint.selectedPointColor : this.m_waypoint.pointColor;
			///Scale of Handles in Scene
			float handleScale = (cameraPos - point.position).magnitude / 50f;

			//if Shift you can active FreeMove like Editor
			if(evt.shift)
			{
				if(selected)
				{
					//Drawing Free Move Handle
					if ((this.m_tool & RTool.MOVE) == RTool.MOVE)
					{
						point.position = Handles.FreeMoveHandle (point.position, Quaternion.identity, handleScale, Vector3.one, Handles.CircleHandleCap);
					}

					//Drawing Free Bezier Handle
					if ((this.m_tool & RTool.BEZIER) == RTool.BEZIER)
					{
						if(point.uniqueTangent)
						{
							point.tangent = Handles.FreeMoveHandle (point.position + point.tangent, Quaternion.LookRotation(cameraPos - point.position), handleScale * 0.5f, Vector3.one, Handles.CircleHandleCap) - point.position;
							point.tangent = -(Handles.FreeMoveHandle (point.position - point.tangent, Quaternion.LookRotation(cameraPos - point.position), handleScale * 0.5f, Vector3.one, Handles.CircleHandleCap) - point.position);
							Handles.DrawLine (point.position + point.tangent, point.position);
							Handles.DrawLine (point.position - point.tangent, point.position);
						}
						else
						{
							point.tangentL = Handles.FreeMoveHandle (point.position + point.tangentL, Quaternion.LookRotation(cameraPos - point.position), handleScale * 0.5f, Vector3.one, Handles.CircleHandleCap) - point.position;
							point.tangentR = Handles.FreeMoveHandle (point.position + point.tangentR, Quaternion.LookRotation(cameraPos - point.position), handleScale * 0.5f, Vector3.one, Handles.CircleHandleCap) - point.position;
							Handles.DrawLine (point.position + point.tangentL, point.position);
							Handles.DrawLine (point.position + point.tangentR, point.position);
						}
					}
				}
				else
				{
					//Drawing SphereButton to Select Points
					if(Handles.Button (point.position, Quaternion.identity, handleScale, handleScale, Handles.SphereHandleCap))
					{
						this.m_index = index;
						this.m_relist.index = index;
						Tools.hidden = true;
						this.Repaint ();
					}
				}
			}
			else
			{
				if(selected)
				{
					//Drawing Move Handle
					if ((this.m_tool & RTool.MOVE) == RTool.MOVE)
					{
						point.position = Handles.DoPositionHandle (point.position, Quaternion.identity);
					}

					//Drawing Bezier Handle
					if ((this.m_tool & RTool.BEZIER) == RTool.BEZIER)
					{
						if(point.uniqueTangent)
						{
							point.tangent = Handles.DoPositionHandle (point.position + point.tangent, Quaternion.identity) - point.position;
							point.tangent = -(Handles.DoPositionHandle (point.position - point.tangent, Quaternion.identity) - point.position);
							Handles.DrawLine (point.position + point.tangent, point.position);
							Handles.DrawLine (point.position - point.tangent, point.position);
						}
						else
						{
							point.tangentL = Handles.DoPositionHandle (point.position + point.tangentL, Quaternion.identity) - point.position;
							point.tangentR = Handles.DoPositionHandle (point.position + point.tangentR, Quaternion.identity) - point.position;
							Handles.DrawLine (point.position + point.tangentL, point.position);
							Handles.DrawLine (point.position + point.tangentR, point.position);
						}
					}
				}

				if(Handles.Button (point.position, Quaternion.identity, handleScale, handleScale, Handles.SphereHandleCap))
				{
					if(this.m_tool == RTool.VIEW || this.m_tool == RTool.SETTINGS)
					{
						this.m_lastTool = (this.m_lastTool == RTool.VIEW || this.m_lastTool == RTool.SETTINGS) ? RTool.MOVE : this.m_lastTool;
						this.m_tool = this.m_lastTool;
					}
					this.m_index = index;
					this.m_relist.index = index;
					Tools.hidden = true;
					this.Repaint ();
				}
			}
			return point;
		}

		//Method Callback to DrawCurve in WaypointUtilty
		private void DrawLine(Vector3 pointA, Vector3 pointB)
		{
			if(this.m_waypoint.drawTrailAA)
			{
				Handles.DrawAAPolyLine (this.m_waypoint.trailAAWidth, new Vector3[] { pointA, pointB });
			}
			else
			{
				Handles.DrawLine (pointA, pointB);
			}
		}

		/// Function to Draw Handles in Scene
		private void OnSceneGUI()
		{
			Event evt = Event.current;
			Vector3 cameraPos = SceneView.currentDrawingSceneView.camera.transform.position;
			if(this.m_waypoint.waypointData != null)
			{
				Handles.color = this.m_waypoint.trailColor;
				///Draw Curves
				for (int i = 0; i < this.m_waypoint.waypointData.length; i++)
				{
					//Drawing a last Curve from Last Point to a FirstPoint
					if(i == this.m_waypoint.waypointData.length - 1)
					{
						if(this.m_waypoint.completeTrail)
							WaypointUtility.DrawCurve(this.m_waypoint.TransformPoint(this.m_waypoint.waypointData[i + 0]), this.m_waypoint.TransformPoint(this.m_waypoint.waypointData[0 + 0]), this.m_waypoint.trailDetailLevel, this.DrawLine);
					}
					else
					{
						WaypointUtility.DrawCurve(this.m_waypoint.TransformPoint(this.m_waypoint.waypointData[i + 0]), this.m_waypoint.TransformPoint(this.m_waypoint.waypointData[i + 1]), this.m_waypoint.trailDetailLevel, this.DrawLine);
					}
				}

				//Drawing Handles to Move the Point of Waypoint
				for(int i = 0; i < this.m_waypoint.waypointData.length; i++)
				{
					EditorGUI.BeginChangeCheck ();
					Point point = this.m_waypoint.waypointData[i];
					Point np = this.m_waypoint.InverseTransformPoint(this.HandleDrawPoint(i, this.m_waypoint.TransformPoint(point), cameraPos, this.m_index == i));

					//Recording Undo
					if(EditorGUI.EndChangeCheck())
					{
						Undo.RecordObjects (new Object[]{this.m_waypoint.waypointData, this}, "Point Modify");
						this.m_waypoint.waypointData[i] = np;
						this.Repaint();
					}
				}
				Handles.color = Color.white;

				if(evt.type == EventType.ExecuteCommand)
				{
					if(evt.commandName == "Duplicate")
					{
						this.OnAddPoint (this.m_relist);
						evt.Use ();
					}
					else if(evt.commandName == "SoftDelete")
					{
						this.OnRemovePoint (this.m_relist);
						evt.Use ();
					}
				}
			}
		}
		#endregion

		#region STATIC METHODS
		private static string GetNameAsset()
		{
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			path = (string.IsNullOrEmpty (path) ? "Assets/" : path);
			string dataPath = Path.GetDirectoryName (Application.dataPath);
			string rpath = Path.Combine (dataPath, path);

			int count = 0;
			foreach(string nfile in Directory.GetFiles(rpath))
			{
				string filename = Path.GetFileName (nfile);
				if(filename.StartsWith("WayPointData") && filename.EndsWith(".asset"))
				{
					count++;
				}
			}

			if(count > 0)
			{
				return Path.Combine (path, string.Format ("WayPointData {0}.asset", count));
			}
			return Path.Combine (path, "WayPointData.asset");
		}

		//Method to Create WayPointData in Asset
		[MenuItem("Waypoint/WaypointData")]
		private static void CreateWaypointData ()
		{
			//Get WaypointData name
			//Because in folder can contain more WaypointDatas
			//I really i dont know if there's a method to do it
			string path = GetNameAsset();
			WaypointData data = ScriptableObject.CreateInstance<WaypointData>();
			AssetDatabase.CreateAsset(data, path);
		}

		//Method to Create WaypointManager in Scene
		[MenuItem("Waypoint/WaypointManager")]
		private static void CreateWaypoint ()
		{
			GameObject obj = new GameObject ();
			obj.AddComponent<WaypointManager> ();
			obj.name = "WayPoint";
			Selection.activeGameObject = obj;

			if(Selection.activeGameObject != null)
				obj.transform.SetParent (Selection.activeGameObject.transform);
		}
		#endregion
	}
}
