using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WalkedCell {
	public Point location;
	public WalkedCell parent;

	public WalkedCell(int x, int y, WalkedCell parent) {
		this.location = new Point (x, y);
		this.parent = parent;
	}
}

public class Warrior : MonoBehaviour {

	public float speed;
	[HideInInspector]
	public int x, y;
	public Point moveTarget;

	private CharacterController characterController;
	private Animator anim;
	private Cell[,] grid;

	List<Point> route = new List<Point> ();
	int currentTarget = 0;

	List<WalkedCell> walkedCells = new List<WalkedCell> ();

	[HideInInspector]
	public bool isEnemy = false;


	void Start () {
		characterController = GetComponent<CharacterController> ();
		anim = GetComponent<Animator> ();
		if (!isEnemy) {
			grid = GameManager.instance.Grid;
		} else {
			var gameGrid = GameManager.instance.Grid;
			var width = gameGrid.GetLength (0);
			var height = gameGrid.GetLength (1);
			grid = new Cell[width, height];

			for (var i = 0; i < width; i++) {
				for (var j = 0; j < height; j++) {
					grid [i, j] = gameGrid [i, height - j - 1];
				}
			}
		}

		Invoke ("StartMove", 1f);
	}
	
	void Update () {

	}

	void StartMove() {
		getRoute ();

		anim.SetBool ("move", true);
		StartCoroutine (Move (3));
	}

	void getRoute() {
		var width = grid.GetLength (0);
		var height = grid.GetLength (1);
		var result = new bool [width, height];

		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height/2; j++) {
				result [i, j] = !grid [i, j].obstacle;
			}
		}

		var aStar = new aStarFinder (new Point (x, y), result);
		route = aStar.FindPath ();
	}


	IEnumerator Move(float time)
	{
		if (currentTarget < route.Count) {
			moveTarget = route [currentTarget];
			currentTarget++;
		} else {
			if (y == grid.GetLength (1) - 1) {
				//WIN
				print("WIN");
				anim.SetBool ("move", false);
				Destroy (gameObject, 1f);
				yield break;
			}

			if (y == 4) 
				walkedCells.Add (new WalkedCell(x, y, null));
			moveTarget = getMoveTarget ();
		}

		var pos = grid [moveTarget.x, moveTarget.y].transform.position;
		pos.y = transform.position.y;
		var angle = Vector3.Angle (transform.forward, pos - transform.position);
		if (angle > 20) {
			var rotation = Quaternion.LookRotation (pos - transform.position);
			StartCoroutine (RotateToTarget (rotation, 0.5f));
		}

		var elapsedTime = 0f;
		var startingPos = transform.position;
		var dir = pos - startingPos;
		var speed = dir.magnitude / time;
		while (elapsedTime < time)
		{
			characterController.SimpleMove(dir.normalized * speed);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		x = moveTarget.x;
		y = moveTarget.y;

		StartCoroutine (Move (3));
	}

	IEnumerator RotateToTarget(Quaternion rotation, float time) {
		var elapsedTime = 0f;
		while (elapsedTime < time) {
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, elapsedTime / time);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}


	Point getMoveTarget() {
		if (isMoveAllow(x, y + 1)) 
			return new Point (x, y + 1);

		if (x >= 0 && x < 4) {
			if (isMoveAllow(x + 1, y))
				return new Point (x + 1, y);
			else if (x > 0 && isMoveAllow(x - 1, y))
				return new Point (x - 1, y);
		} else {
			if (isMoveAllow(x - 1, y))
				return new Point (x - 1, y);
			else if (x < 7 && isMoveAllow(x + 1, y))
				return new Point (x + 1, y);
		}

		walkedCells.Add (new WalkedCell(x, y - 1, walkedCells [walkedCells.Count - 1]));
		return new Point (x, y - 1);
	}

	bool isMoveAllow(int x, int y) {
		if (!grid [x, y].obstacle && walkedCells.FindIndex (c => c.location.x == x && c.location.y == y) == -1) {
			walkedCells.Add (new WalkedCell(x, y, walkedCells [walkedCells.Count - 1]));
			return true;
		}

		return false;
	}

}
