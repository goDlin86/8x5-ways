using UnityEngine;
using System.Collections;

public class ArcherFogOfWarVisibility : MonoBehaviour
{
	private bool observed = false;

	Renderer renderer;

	// Use this for initialization
	void Start () {
		renderer = transform.parent.Find ("Archer001").GetComponent<Renderer> ();
	}

	void Update()
	{
		if(observed)
		{
			renderer.enabled = true;
		}
		else
		{
			renderer.enabled = false;
		}

		observed = false;
	}
	
	void Observed()
	{
		observed = true;
	}
}
