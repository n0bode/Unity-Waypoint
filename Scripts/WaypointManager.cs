/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WayPoint
{
	[AddComponentMenu("Waypoint/WaypointManager")]
	public class WaypointManager : MonoBehaviour
	{
		public WaypointData waypointData;
		public bool completeTrail = true;
		public bool drawTrailAA = true;
		public bool drawTrailGizmos = true;
		[HideInInspector]
		public bool drawTrail = true;
		public int trailAAWidth = 2;
		public int trailDetailLevel = 20;
		public Color trailColor = Color.white;
		public Color pointColor = new Color32 (18, 192, 232, 255);
		public Color selectedPointColor = Color.HSVToRGB(33, 77, 90);
		public Color tangentColor = Color.HSVToRGB (145, 73, 96);

		// Use this for initialization
		void Start ()
		{

		}

		private int NormalizeIndex (int index, bool loop)
		{
			if (index == this.waypointData.length)
			{
				if(loop)
				{
					return 0;
				}
				return this.waypointData.length - 1;
			}
			return index;
		}

		//Limits Factor from 0 to 1
		private float NormalizeFactor(float factor)
		{
			if(Mathf.Abs(factor) > 1f)
				factor = (factor - Mathf.FloorToInt (factor));

			return factor < 0 ? factor + 1f: factor;
		}

		private Vector3 GetPosition(float factor, bool loop)
		{
			int len = loop ? this.waypointData.length : this.waypointData.length - 1;
			int i0 = this.NormalizeIndex(Mathf.FloorToInt (factor * len), loop);
			int i1 = this.NormalizeIndex(i0 + 1, loop);
			//This is like: (currentIndexPoint - lastIndexPoint) / (nextIndexPoint - lastIndexPoint)
			//currentIndexPoint = factor * lengthWaypoint
			//lastIndexPoint = floor(factor * lengthWaypoint)
			//nextIndexPoint = lastIndexPoint + 1
			float time = (factor * len) % 1;

			Point p0 = this.TransformPoint(this.waypointData [i0]);
			Point p1 = this.TransformPoint(this.waypointData [i1]);
			Vector3 tan0 = p0.position + (p0.uniqueTangent ? p0.tangent : p0.tangentR);
			Vector3 tan1 = p1.position + (p1.uniqueTangent ? -p1.tangent : p1.tangentL);
			return WaypointUtility.CubicBezier (p0.position, tan0, tan1, p1.position, time);
		}

		/// <summary>
		/// Gets the world space on trail based on Factor like in a Curve
		/// </summary>
		/// <returns>World Space</returns>
		/// <param name="fractor">Fractor.</param>
		/// <param name="loop">If set to <c>true</c> loop.</param>
		public Vector3 GetPositionOnTrail(float fractor, bool loop=true)
		{
			if(this.waypointData == null)
			{
				return this.transform.position;
			}
			return this.GetPosition (this.NormalizeFactor(fractor), loop);
		}

		/// <summary>
		/// Transform World Point to Point Local
		/// </summary>
		/// <returns>Local Point</returns>
		/// <param name="point">World Point</param>
		public Point TransformPoint(Point point)
		{
			Point np = new Point(point);
			np.position = this.transform.TransformPoint(point.position);
			np.tangentL = this.transform.TransformVector(point.tangentL);
			np.tangentR = this.transform.TransformVector(point.tangentR);
			np.tangent  = this.transform.TransformVector(point.tangent);
			return np;
		}

		/// <summary>
		/// Transform Local Point to World Point
		/// </summary>
		/// <returns>World Point</returns>
		/// <param name="point">Local Point</param>
		public Point InverseTransformPoint(Point point)
		{
			Point np = new Point(point);
			np.position = this.transform.InverseTransformPoint(point.position);
			np.tangentL = this.transform.InverseTransformVector(point.tangentL);
			np.tangentR = this.transform.InverseTransformVector(point.tangentR);
			np.tangent  = this.transform.InverseTransformVector(point.tangent);
			return np;
		}

		void OnDrawGizmos()
		{
			if (!this.drawTrail || this.trailDetailLevel == 0 || !this.drawTrailGizmos)
				return;

			if(this.waypointData != null && Event.current.type == EventType.Repaint)
			{
				Gizmos.color = this.trailColor;

				for(int i = 0; i < this.waypointData.length; i++)
				{
					if(i == this.waypointData.length - 1)
					{
						if(this.completeTrail)
							WaypointUtility.DrawCurve(this.TransformPoint(this.waypointData[i + 0]), this.TransformPoint(this.waypointData[0 + 0]), this.trailDetailLevel, Gizmos.DrawLine);
					}
					else
					{
						WaypointUtility.DrawCurve(this.TransformPoint(this.waypointData[i + 0]), this.TransformPoint(this.waypointData[i + 1]), this.trailDetailLevel, Gizmos.DrawLine);
					}
				}
				Gizmos.color = Color.white;
			}
		}
	}
}
