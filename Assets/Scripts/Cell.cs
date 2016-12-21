using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {

	Renderer r;
	public int x;
	public int y;
	public bool obstacle = false;


	void Start () {
		r = GetComponent<Renderer> ();
	}
	

	void Update () {
	
	}

	public void ToggleCell () {
		if (obstacle) {
			r.material.SetColor("_BaseColor", new Color (0, 0.8f, 0, 0.25f));
		} else {
			r.material.SetColor("_BaseColor", new Color (0, 0.6f, 0, 0.8f));
		}

		obstacle = !obstacle;
	}
}
