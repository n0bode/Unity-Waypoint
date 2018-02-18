/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WayPoint;

namespace WayPoint
{
	[AddComponentMenu("Waypoint/WayPointAgent")]
	public class WaypointAgent : MonoBehaviour
	{
		public WaypointManager manager;
		public float factor = 0f;
		public float speed = 1f;
		public float height = 2f;
		public float baseOffset = 0f;
		public float radius = 1f;
		public bool completeTrail = true;
		public bool loop = true;
		[HideInInspector]
		public bool isStopped = false;

		//Previous Factor
		private float m_factor = 0f;

		// Use this for initialization
		void Start ()
		{
			this.UpdatePosition ();
		}

		// Update is called once per frame
		void FixedUpdate ()
		{
			this.UpdatePosition ();
		}

		public void UpdatePosition()
		{
			if (this.manager != null)
			{
				if(!this.isStopped && Application.isPlaying)
				{
					this.factor += this.speed * Time.deltaTime / 10f;
					if(!this.loop)
					{
						this.factor = Mathf.Clamp01 (this.factor);
					}
				}

				if (this.m_factor != this.factor)
				{
					this.OnChangePosition (this.manager.GetPositionOnTrail (this.factor, this.completeTrail));
					this.m_factor = this.factor;
				}
			}
		}

		public void OnChangePosition(Vector3 pos)
		{
			Vector3 lpos = this.manager.GetPositionOnTrail (this.m_factor, this.completeTrail);
			this.SetPosition(pos);
			this.transform.rotation = Quaternion.LookRotation (pos - lpos);
		}

		public void SetPosition(Vector3 pos)
		{
			this.transform.position = pos - this.GetBaseOffset();
		}

		void DrawCapsule(Vector3 pos, float height, float radius, Color color, int detail=6)
		{
			Color save = Gizmos.color;
			Gizmos.matrix = Matrix4x4.TRS (pos, this.transform.rotation, Vector3.one);
			Gizmos.color = color;
			Vector3 offset = Vector3.up * height;
			for(int i = 0; i < detail; i++)
			{
				float f0 = (i + 0) / (float)(detail - 1f) * Mathf.PI * 2f;
				float f1 = (i + 1) / (float)(detail - 1f) * Mathf.PI * 2f;
				Vector3 v0 = new Vector3 (Mathf.Sin (f0) * radius, 0, Mathf.Cos (f0) * radius);
				Vector3 v1 = v0 + offset;
				Vector3 v2 = new Vector3 (Mathf.Sin (f1) * radius, 0, Mathf.Cos (f1) * radius);
				Vector3 v3 = v2 + offset;

				if(i % (detail / 4) == 0)
					Gizmos.DrawLine (v0 , v1);
				Gizmos.DrawLine (v0, v2);
				Gizmos.DrawLine (v1, v3);
			}
			Gizmos.color = save;
			Gizmos.matrix = Matrix4x4.identity;
		}

		Vector3 GetBasePosition()
		{
			return this.transform.position + this.GetBaseOffset();
		}

		Vector3 GetBaseOffset()
		{
			return Vector3.down * this.baseOffset;
		}

		void OnDrawGizmos()
		{
			this.DrawCapsule (this.GetBasePosition(), this.height, this.radius, new Color(0.8f, 0.2f, 0.4f, 1f), 13);
		}
	}

}
