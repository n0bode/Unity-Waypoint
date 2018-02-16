using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nWayPoint;

public class SimpleAgent : MonoBehaviour
{
	[System.Serializable]
	public class ToggleGroup
	{
		public bool x = true;
		public bool y = true;
		public bool z = true;
	}
		
	public Waypoint waypoint;
	public float factor = 0;
	public bool loop = true;
	public float acceleration = 1f;
	public float speedMove = 2f;
	public float height = 2f;
	private float m_lfactor = 0f;
	private float m_speed = 0f;
	private bool m_isStoped = false;

	// Use this for initialization
	void Start () 
	{
		if(this.waypoint != null)
		{
			this.transform.position = this.waypoint.GetPositionOnTrail (this.factor, this.loop);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(this.waypoint != null)
		{
			if(!this.m_isStoped)
			{
				this.m_speed = this.acceleration * this.speedMove * Time.deltaTime;
				this.m_speed = Mathf.Clamp (this.m_speed, -this.speedMove * Time.deltaTime, this.speedMove * Time.deltaTime);
				this.factor += this.m_speed * Time.deltaTime;
			}

			if(this.m_lfactor != this.factor)
			{
				this.ChangePosition(this.waypoint.GetPositionOnTrail (this.factor, this.loop));
				this.m_lfactor = this.factor;
			}

		}
	}

	public void Stop()
	{
		this.m_isStoped = true;
	}

	public void Return()
	{
		this.m_isStoped = false;
	}

	public void ChangePosition (Vector3 pos)
	{
		Vector3 lpos = this.waypoint.GetPositionOnTrail (this.m_lfactor);
		this.transform.rotation = Quaternion.LookRotation (pos - lpos);

		RaycastHit hit;
		if(Physics.Raycast(pos, Vector3.down, out hit, 10))
		{
			this.transform.position = hit.point + hit.normal * this.height;
		}
	}
}
