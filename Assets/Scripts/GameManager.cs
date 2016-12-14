using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;

public class GameManager : MonoBehaviour {

	const int width = 8;
	const int height = 5;

	const float scaleFactor = 0.7f;

	public GameObject prefabCell;
	public GameObject prefabMyGrid;
	public GameObject prefabEnemyGrid;
	public GameObject prefabWarrior;
	public GameObject prefabWarriorEnemy;
	public GameObject gameUI;
	public GameObject tooltipUI;
	public GameObject[] prefabObstacles;

	public Text timerText;
	int timer = 10;

	public Cell[,] Grid = new Cell[width, height * 2];
	List<Warrior> myWarriors = new List<Warrior> ();
	List<Warrior> enemyWarriors = new List<Warrior> ();

	int obstacleCount = 0;
	const int obstacleMaxCount = 7;
	public Text obstacleCountText;

	int[] obstacleRowCount = new int[height - 2];
	const int obstacleMaxRowCount = 4;

	Vector3 lastObjPos = Vector3.zero;

	Color redColor = new Color (0.8f, 0, 0, 0.25f);
	Color greenColor = new Color (0, 0.8f, 0, 0.25f);
	Color setColor = new Color (0, 0.6f, 0, 0.8f);

	float deltaTime = 0.0f;

	private static GameManager sInstance;
	public static GameManager instance
	{
		get{
			if(sInstance == null)
				Debug.LogError("Scene missing GameController Object.");

			return sInstance;
		}
	}

	void Awake()
	{
		if(sInstance != null)
			Debug.LogError("ERROR: More than one GameController in scene. There can only be one!");
		sInstance = this;

		Application.targetFrameRate = 60;
	}

	void OnDestroy()
	{
		sInstance = null; 
	}

	void Start () {
		CreateGrid ();

		//timer
		gameUI.GetComponent<Animation> ().Play ("PanelIn");
		Invoke("UpdateTimer", 2f);

		for (var i = 0; i < height - 2; i++) {
			obstacleRowCount[i] = 0;
		}
	}
	
	void Update () {
		
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	void UpdateTimer () {
		timer--;
		timerText.text = timer.ToString();

		if (timer > 0) {
			Invoke ("UpdateTimer", 1f);
		} else {
			gameUI.GetComponent<Animation> ().Play ("PanelOut");
			LeanTouch.OnFingerSet -= OnFingerSet;
			CreateObstacles ();
		}
	}



	protected virtual void OnEnable()
	{
		LeanTouch.OnFingerTap += OnFingerTap;
		LeanTouch.OnFingerSet += OnFingerSet;
		LeanTouch.OnFingerUp  += OnFingerUp;
	}

	protected virtual void OnDisable()
	{
		LeanTouch.OnFingerTap -= OnFingerTap;
		LeanTouch.OnFingerSet -= OnFingerSet;
		LeanTouch.OnFingerUp  -= OnFingerUp;
	}



	void CreateGrid () {
		//my grid
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var x = scaleFactor * (i - width / 2) + scaleFactor / 2;
				var z = scaleFactor * (j - height + 0.5f);
				var pos = new Vector3 (x, 0.1f, z);
				var cell = Instantiate (prefabCell, pos, Quaternion.Euler(90, 0, 0)) as GameObject;
				cell.transform.parent = prefabMyGrid.transform;

				var r = cell.GetComponent<Renderer> ();
				if (j == 0 || j == height - 1) {
					//r.material.color = redColor;
					//if (j == 0) {
					//	cell.layer = 9;
					//}
				} else {
					r.material.color = greenColor;
					cell.layer = 8;
				}

				Grid [i, j] = cell.GetComponent<Cell>();
				Grid [i, j].x = i;
				Grid [i, j].y = j;
			}
		}

		//enemy grid
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var x = scaleFactor * (i - width / 2) + scaleFactor / 2;
				var z = scaleFactor * (height - j - 0.5f);
				var pos = new Vector3 (x, 0.1f, z);
				var cell = Instantiate (prefabCell, pos, Quaternion.Euler(90, 0, 0)) as GameObject;
				cell.transform.parent = prefabEnemyGrid.transform;

				Grid [i, 2 * height - 1 - j] = cell.GetComponent<Cell>();
				Grid [i, 2 * height - 1 - j].x = i;
				Grid [i, 2 * height - 1 - j].y = 2 * height - 1 - j;
			}
		}
	}


	void CreateObstacles() {
		//create random enemy obstacles
		var countEnemyObstacles = 0;
		while (countEnemyObstacles < 7) {
			var x = Random.Range (0, 1000) % width;
			var y = height + 1 + Random.Range (0, 1000) % 3;
			if (!Grid [x, y].obstacle) {
				Grid [x, y].obstacle = true;
				countEnemyObstacles++;
			}
		}

		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var cell = Grid [i, j];
				var r = cell.GetComponent<Renderer> ();
				if (j == 0) {
					r.material.color = greenColor;
					cell.gameObject.layer = 9;
				} else {
					r.material.color = new Color (0, 0, 0, 98f / 255f);
				}
			}
		}


		for (var i = 0; i < width; i++) {
			for (var j = 1; j < height * 2 - 1; j++) {
				if (Grid [i, j].obstacle) {
					var prefab = prefabObstacles [Random.Range (0, 1000) % 5];
					var obj = Instantiate (prefab, Grid [i, j].transform.position, Quaternion.Euler (-90f, 0, 0)) as GameObject;
					if (obj.name.Contains("Tree0"))
						obj.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
					else
						obj.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
				}
			}
		}

		StartCoroutine (CreateEnemies ());
	}
		

	IEnumerator CreateEnemies() {
		while (enemyWarriors.Count < 5) {
			Cell c = Grid [Random.Range(0, width - 1), height * 2 - 1];
			var pos = c.transform.position;
			pos.y = 0.8f;

			var war = Instantiate (prefabWarriorEnemy, pos, Quaternion.identity) as GameObject;
			var w = war.GetComponent<Warrior> ();
			w.x = c.x;
			w.y = 0;
			w.isEnemy = true;
			enemyWarriors.Add (w);

			yield return new WaitForSeconds (Random.Range (0.5f, 2f));
		}

		yield break;
	}



	private void OnFingerSet(LeanFinger finger) {
		var ray = finger.GetRay();
		var hit = default(RaycastHit);

		LayerMask layerMask = 1 << 8;
		if (Physics.Raycast (ray, out hit, float.PositiveInfinity, layerMask) == true) {
			var obj = hit.collider.gameObject;
			if (obj.transform.position != lastObjPos) {
				lastObjPos = obj.transform.position;

				Cell c = obj.GetComponent<Cell> ();
				if (obstacleRowCount [c.y - 1] == obstacleMaxRowCount) {
					tooltipUI.GetComponent<Animation> ().Play ();
				}

				if (c.obstacle || ((!c.obstacle && obstacleCount < obstacleMaxCount) && obstacleRowCount [c.y - 1] < obstacleMaxRowCount)) {
					c.ToggleCell ();

					if (c.obstacle) {
						obstacleCount++;
						obstacleRowCount[c.y - 1]++;
					} else {
						obstacleCount--;
						obstacleRowCount[c.y - 1]--;
					}

					var count = obstacleMaxCount - obstacleCount;
					obstacleCountText.text = count.ToString ();
				}

			}
		}
	}

	private void OnFingerUp(LeanFinger finger) {
		lastObjPos = Vector3.zero;
	}

	private void OnFingerTap(LeanFinger finger) {
		var ray = finger.GetRay();
		var hit = default(RaycastHit);

		LayerMask layerMask = 1 << 9;
		if (Physics.Raycast (ray, out hit, float.PositiveInfinity, layerMask) == true) {
			var obj = hit.collider.gameObject;
			Cell c = obj.GetComponent<Cell> ();
			var pos = c.transform.position;
			pos.y = 0.8f;

			var war = Instantiate (prefabWarrior, pos, Quaternion.identity) as GameObject;
			var w = war.GetComponent<Warrior> ();
			w.x = c.x;
			w.y = c.y;
			myWarriors.Add (w);

			obj.GetComponent<Renderer> ().material.color = redColor;
			obj.layer = 1;
		}
	}




	Cell getCell(Vector3 pos) {
		int x = Mathf.RoundToInt(pos.x / scaleFactor + width / 2 - 0.5f);
		int y = Mathf.RoundToInt(pos.z / scaleFactor + height);

		Cell cell = null;
		if (x < width && y < height * 2)
			cell = Grid [x, y];

		return cell;
	}


	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 50;
		style.normal.textColor = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}
