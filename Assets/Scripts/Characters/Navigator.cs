using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WalkedCell {
	public Point location;
	public WalkedCell parent;

	public WalkedCell(int x, int y, WalkedCell parent) {
		this.location = new Point (x, y);
		this.parent = parent;
	}
}

public class Navigator : MonoBehaviour {

	const float time = 3f; //???????

	[HideInInspector]
	public int x, y;
	public Point moveTarget;

	private CharacterController characterController;
	private Cell[,] grid;

	List<Point> route = new List<Point> ();
	int currentTarget = 0;

	List<WalkedCell> walkedCells = new List<WalkedCell> ();

	#region Events
	public delegate void MovingEventHandler(Vector3 targetCoordinate);
	public delegate void EventHandler();
	public event EventHandler OnStartWalk;
	public event MovingEventHandler OnWalking;
	public event EventHandler OnStopWalk;
	#endregion Events


	void Awake () {
		characterController = GetComponent<CharacterController> ();
		//Invoke ("StartMove", 1f);
	}

	public void getRoute() {
		grid = GameManager.instance.Grid;
		var width = grid.GetLength (0);
		var height = grid.GetLength (1);

		if (GetComponent<Warrior> ().isEnemy) {
			var gameGrid = grid;
			grid = new Cell[width, height];

			for (var i = 0; i < width; i++) {
				for (var j = 0; j < height; j++) {
					grid [i, j] = gameGrid [i, height - j - 1];
				}
			}
		}
			
		var result = new bool [width, height];

		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height/2; j++) {
				result [i, j] = !grid [i, j].obstacle;
			}
		}

		var aStar = new aStarFinder (new Point (x, y), result);
		route = aStar.FindPath ();
	}

	public void StartMoving() {
		if (OnStartWalk != null)
			OnStartWalk ();
		StartCoroutine ("Move");
	}

	public void StopMoving() {
		if (OnStopWalk != null)
			OnStopWalk ();
		StopCoroutine ("Move");
	}

	IEnumerator Move()
	{
		if (currentTarget < route.Count) {
			moveTarget = route [currentTarget];
			currentTarget++;
		} else {
			if (y == grid.GetLength (1) - 1) {
				//WIN
				print("WIN");
				if (OnStopWalk != null)
					OnStopWalk ();
				
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
		while (elapsedTime < time) // TODO ???????
		{
			characterController.SimpleMove(dir.normalized * speed);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		x = moveTarget.x;
		y = moveTarget.y;

		StartCoroutine ("Move");
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
