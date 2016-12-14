using UnityEngine;
using System.Collections;

public class FogOfWarSight : MonoBehaviour
{
	public float radius = 10.0f;
	public LayerMask layerMask = -1;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach(Collider col in Physics.OverlapBox(transform.position, new Vector3(radius, radius, radius), Quaternion.identity, layerMask))
		{
			col.SendMessage("Observed",SendMessageOptions.DontRequireReceiver);
		}
	}
}
