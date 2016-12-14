using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState { Untested, Open, Closed }

public struct Point {
	public int x, y;

	public Point (int x, int y) {
		this.x = x;
		this.y = y;
	}
}

public class Node {
	private Node parentNode;
	public Point Location { get; private set; }
	public bool IsWalkable { get; set; }
	public int G { get; private set; }
	public int H { get; private set; }
	public NodeState State { get; set; }
	public float F
	{
		get { return this.G + this.H; }
	}
	public Node ParentNode
	{
		get { return this.parentNode; }
		set
		{
			// When setting the parent, also calculate the traversal cost from the start node to here (the 'G' value)
			this.parentNode = value;
			this.G = this.parentNode.G + 1;
		}
	}

	public Node(int x, int y, int h, bool isWalkable)
	{
		this.Location = new Point(x, y);
		this.State = NodeState.Untested;
		this.IsWalkable = isWalkable;
		this.H = h;
		this.G = 0;
	}
}

public class aStarFinder {

	private int width;
	private int height;
	private Node[,] nodes;
	private Node startNode;
	private Node endNode;

	public aStarFinder(Point start, bool[,] grid)
	{
		InitializeNodes(grid);
		this.startNode = this.nodes[start.x, start.y];
		this.startNode.State = NodeState.Open;
	}

	public List<Point> FindPath()
	{
		// The start node is the first entry in the 'open' list
		List<Point> path = new List<Point>();
		bool success = Search(startNode);
		if (success)
		{
			// If a path was found, follow the parents from the end node to build a list of locations
			Node node = this.endNode;
			while (node.ParentNode != null)
			{
				path.Add(node.Location);
				node = node.ParentNode;
			}

			// Reverse the list so it's in the correct order when returned
			path.Reverse();
		}

		return path;
	}

	private void InitializeNodes(bool[,] map)
	{
		this.width = map.GetLength(0);
		this.height = map.GetLength(1);
		this.nodes = new Node[this.width, this.height];
		for (int y = 0; y < this.height; y++)
		{
			for (int x = 0; x < this.width; x++)
			{
				var countObstacle = 0;
				for (int i = 0; i < this.height; i++) {
					if (!map [x, i]) {
						countObstacle++;
					}
				}
				var h = 5 - y + 2 * countObstacle;
				this.nodes[x, y] = new Node(x, y, h, map[x, y]);
			}
		}
	}

	private bool Search(Node currentNode)
	{
		// Set the current node to Closed since it cannot be traversed more than once
		currentNode.State = NodeState.Closed;
		List<Node> nextNodes = GetAdjacentWalkableNodes(currentNode);

		// Sort by F-value so that the shortest possible routes are considered first
		nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
		foreach (var nextNode in nextNodes)
		{
			// Check whether the end node has been reached
			if (nextNode.Location.y == 4)
			{
				this.endNode = nextNode;
				return true;
			}
			else
			{
				// If not, check the next set of nodes
				if (Search(nextNode)) // Note: Recurses back into Search(Node)
					return true;
			}
		}

		// The method returns false if this path leads to be a dead end
		return false;
	}

	private List<Node> GetAdjacentWalkableNodes(Node fromNode)
	{
		List<Node> walkableNodes = new List<Node>();
		IEnumerable<Point> nextLocations = GetAdjacentLocations(fromNode.Location);

		foreach (var location in nextLocations)
		{
			int x = location.x;
			int y = location.y;

			// Stay within the grid's boundaries
			if (x < 0 || x >= this.width || y < 0 || y >= this.height)
				continue;

			Node node = this.nodes[x, y];
			// Ignore non-walkable nodes
			if (!node.IsWalkable)
				continue;

			// Ignore already-closed nodes
			if (node.State == NodeState.Closed)
				continue;

			// Already-open nodes are only added to the list if their G-value is lower going via this route.
			if (node.State == NodeState.Open)
			{
				float gTemp = fromNode.G + 1;
				if (gTemp < node.G)
				{
					node.ParentNode = fromNode;
					walkableNodes.Add(node);
				}
			}
			else
			{
				// If it's untested, set the parent and flag it as 'Open' for consideration
				node.ParentNode = fromNode;
				node.State = NodeState.Open;
				walkableNodes.Add(node);
			}
		}

		return walkableNodes;
	}

	private static IEnumerable<Point> GetAdjacentLocations(Point fromLocation)
	{
		return new Point[]
		{
			new Point(fromLocation.x-1, fromLocation.y  ),
			new Point(fromLocation.x,   fromLocation.y+1),
			new Point(fromLocation.x+1, fromLocation.y  ),
			new Point(fromLocation.x,   fromLocation.y-1)
		};
	}
}
