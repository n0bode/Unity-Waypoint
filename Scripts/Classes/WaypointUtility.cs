/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1
/// Github Page: https://github.com/n0bode/Unity-Waypoint

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WayPoint
{

	public static class WaypointUtility
	{
		//Delegate to Draw the Curve
		public delegate void DrawLine(Vector3 start, Vector3 end);

		private static float Pow(float num, int exp)
		{
			return Mathf.Pow(num, exp);

		}

		/// <summary>
		/// Quadratics the bezier.
		/// From Wikipedia
		/// </summary>
		/// <returns>The bezier.</returns>
		/// <param name="posA">Position a.</param>
		/// <param name="tan">Tan.</param>
		/// <param name="posB">Position b.</param>
		/// <param name="time">Time.</param>
		public static Vector3 QuadraticBezier(Vector3 posA, Vector3 tan, Vector3 posB, float time)
		{
			return Pow(1 - time, 2) * posA + 2 * time * (1 - time) * tan + Pow(time, 2) * posB;
		}

		/// <summary>
		/// Cubics the bezier
		/// From Wikipedia
		/// </summary>
		/// <returns>The bezier.</returns>
		/// <param name="posA">Position a.</param>
		/// <param name="tanA">Tan a.</param>
		/// <param name="tanB">Tan b.</param>
		/// <param name="posB">Position b.</param>
		/// <param name="time">Time.</param>
		public static Vector3 CubicBezier(Vector3 posA, Vector3 tanA, Vector3 tanB, Vector3 posB, float time)
		{
			return Pow(1 - time, 3) * posA + 3 * time * Pow(1 - time, 2) * tanA + 3 * Pow(time, 2) * (1 - time) * tanB + Pow(time, 3) * posB;
		}

		/// <summary>
		/// Draws the bezier curve
		/// </summary>
		/// <param name="pointA">Point a.</param>
		/// <param name="pointB">Point b.</param>
		/// <param name="detail">Level Detail of Curve</param>
		/// <param name="drawLine">Function to Draw the Curve</param>
		public static void DrawCurve(Point pointA, Point pointB, int detail, DrawLine drawLine)
		{
			//float dis = Mathf.Clamp((pointB.position - pointA.position).magnitude, 0.5f, 1);
			Vector3 tan0 = pointA.position + (pointA.uniqueTangent ? pointA.tangent : pointA.tangentR);
			Vector3 tan1 = pointB.position + (pointB.uniqueTangent ? -pointB.tangent : pointB.tangentL);

			if(tan0 == pointA.position && tan1 == pointB.position)
			{
				drawLine (pointA.position, pointB.position);
			}
			else
			{
				int max = Mathf.RoundToInt (detail /** dis*/);
				Vector3 lpos = pointA.position;
				for(int i = 1; i < max + 1; i++)
				{
					float step = (float)i / (max);
					Vector3 npos = WaypointUtility.CubicBezier(pointA.position, tan0, tan1, pointB.position, step);
					drawLine(lpos, npos);
					lpos = npos;
				}
			}
		}
	}
}
