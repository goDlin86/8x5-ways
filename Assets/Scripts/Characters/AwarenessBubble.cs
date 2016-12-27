using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AwarenessBubble : MonoBehaviour {
	
	public Warrior my;
	private List<GameObject> targets;

	void Awake()	
	{
		targets = new List<GameObject>();
	}
	
	public GameObject GetTarget()
	{
		if(targets.Count > 0)
		{
			foreach(GameObject target in targets)
			{
				if(target != null)
					return target;
			}
			targets.TrimExcess();
		}
		
		return null;
	}
		                     
	void OnTriggerEnter(Collider col)
	{
		Warrior t = col.GetComponent<Warrior>();
		if(t != null && t.isEnemy != my.isEnemy)
			targets.Add(t.gameObject);
	}
	
	void OnTriggerExit(Collider col)
	{
		Warrior t = col.GetComponent<Warrior>();
		if(t != null && t.isEnemy != my.isEnemy)
			targets.Remove(t.gameObject);
	}
}