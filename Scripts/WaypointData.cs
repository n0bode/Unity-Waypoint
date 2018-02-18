/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WayPoint
{
	[System.Serializable]
	public class WaypointData : ScriptableObject, IEnumerable
	{
		[HideInInspector]
		public Point[] m_points = new Point[0]; //< List of Points
		[SerializeField , HideInInspector]
		private int m_lastid = 0; //< I'm thinking in future, i'll do this

		/// <summary>
		/// Return the length of this data
		/// </summary>
		/// <value>length of list of points</value>
		public int length
		{
			get{ return this.m_points.Length; }
		}

		/// <summary>
		/// Gets or sets the <see cref="nWayPoint.WaypointData"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public Point this[int index]
		{
			get{ return this.m_points[index];  }
			set{ this.m_points[index] = value; }
		}

		/// <summary>
		/// Gets the last point
		/// </summary>
		/// <value>The last point</value>
		public Point last
		{
			get{ return this.m_points[this.length - 1]; }
		}

		/// <summary>
		/// Adds a new point to this data
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="point">Point.</param>
		public Point AddPoint(Point point)
		{
			Array.Resize<Point>(ref this.m_points, this.length + 1);
			point.id = this.m_lastid++;
			this.m_points[this.length - 1] = point;
			return point;
		}

		/// <summary>
		/// Duplicate the specified index
		/// </summary>
		/// <param name="index">Index.</param>
		public Point Duplicate(int index)
		{
			Point point = new Point(this.m_points [index]);
			point.id = this.m_lastid++;

			Point[] points = this.m_points;
			Array.Resize<Point> (ref this.m_points, this.length + 1);
			for(int i = index; i < this.length - 1; i++)
			{
				this.m_points [i + 1] = points [i];
			}
			this.m_points [index + 1] = point;
			return point;
		}

		/// <summary>
		/// Removes the point.
		/// </summary>
		/// <param name="point">Point.</param>
		public void RemovePoint(Point point)
		{
			Point[] saved = this.m_points;
			int jump = 0;

			Array.Resize<Point>(ref this.m_points, this.length - 1);
			for(int i = 0; i < this.length; i++)
			{
				if(this.m_points[i].id == point.id)
				{
					jump++;
				}
				this.m_points[i] = saved[i + jump];
			}
		}

		public void RemovePoint(int index)
		{
			Point[] saved = this.m_points;
			int jump = 0;

			Array.Resize<Point>(ref this.m_points, this.length - 1);
			for(int i = 0; i < this.length; i++)
			{
				if(i == index)
				{
					jump++;
				}
				this.m_points[i] = saved[i + jump];
			}
		}

		#region IEnumerable implementation
		public IEnumerator GetEnumerator ()
		{
			return this.m_points.GetEnumerator();
		}
		#endregion
	}
}
