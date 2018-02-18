/// Author: Paulo Camacan (N0bode)
/// Unity Version: 5.6.2f1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WayPoint
{
	[System.Serializable]
	public sealed class Point
	{
		public string name; //< i'll use this in a future update
		public bool uniqueTangent; //< Only a tangent
		public Vector3 position; //< position of point. ps: World
		public Vector3 tangentL; //< Left Tangent
		public Vector3 tangentR; //< Right Tangent
		public Vector3 tangent; //< Just use it when uniqueTangent is true

		[HideInInspector]
		public int id; //<< i'll use this in a future update

		public Point()
		{
			this.position = Vector3.zero;
			this.tangentL = Vector3.zero;
			this.tangentR = Vector3.zero;
			this.tangent = Vector3.zero;
			this.uniqueTangent = true;
		}

		public Point(Point point)
		{
			this.position = point.position;
			this.tangentL = point.tangentL;
			this.tangentR = point.tangentR;
			this.tangent = point.tangent;
			this.uniqueTangent = point.uniqueTangent;
			this.name = point.name;
			this.id = point.id;
		}

		public Point(Vector3 pos)
		{
			this.position = pos;
			this.tangentL = Vector3.zero;
			this.tangentR = Vector3.zero;
		}
	}
}
