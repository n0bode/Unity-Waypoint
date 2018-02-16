using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAt : MonoBehaviour 
{
	public Transform target;

	// Update is called once per frame
	void Update () 
	{
		this.LookAt ();
	}

	public void LookAt()
	{
		if(this.target != null)
		{
			this.transform.LookAt (this.target);	
		}
	}
}
