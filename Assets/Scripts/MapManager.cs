using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{

	public int column = 20;
	public int rows = 20;
	public GameObject[] floorTiles;
	public GameObject wall;
	public GameObject spawn;
	public Vector3 tileSizeInUnits = new Vector3(1.0f, 0.57f, 0.5f);
	[Range(0f, 0.5f)]
	public float perlinOffset = 0.38f;
	[Range(0, 1)]
	public float perlinRange = 0.11f;

	private GameObject[][] map = null;
	private uint floor;

	//debug
	public GameObject closest;

	public void MapSetup(uint level)
	{
		floor = 0;
		// remove old map if there is any
		if (map != null)
		{
			for (int x = 0; x < map.Length; ++x)
			{
				for (int y = 0; y < map[x].Length; ++y)
				{
					Destroy(map[x][y]);
				}
			}
		}
		// allocate map array
		map = new GameObject[column][];
		for (int i = 0; i < column; ++i)
		{
			map[i] = new GameObject[rows];
		}

		// generate map with using perlin noise
		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < column; x++)
			{
				GameObject instance;
				float seed = (float)Network.time + 0.1f;
				float p = Mathf.PerlinNoise((float)x * perlinRange + seed, (float)y * perlinRange + seed);
				if (p > perlinOffset)
				{
					int index = 1;
					if (p < perlinOffset + perlinOffset / 1.6f)
						index = 0;
					instance = Instantiate(floorTiles[index], toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
					++floor;
				}
				else
				{
					instance = Instantiate(wall, toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
				}
				map[x][y] = instance;
			}
		}

		removeSmallIslands();

		// if the map is too small, regenerate it
		if (floor < column * rows / 3f)
			MapSetup(level);

		connectIslands();

		// Setup spawn
		bool found = false;
		for (int x = 0; x < map.Length; ++x)
		{
			for (int y = 0; y < map[x].Length; ++y)
			{
				if (map[x][y].layer == LayerMask.NameToLayer("Floor"))
				{
					Destroy(map[x][y]);
					map[x][y] = Instantiate(spawn, toIsometric(new Vector3(x, y, 0f)), Quaternion.identity) as GameObject;
					found = true;
					spawn = map[x][y];
					break;
				}
			}
			if (found)
				break;
		}
	}

	GameObject instance;
	List<Vector3> path;
	List<GameObject> tileBackup = new List<GameObject>();
	private void Update()
	{
		// debug mouse location
		if (instance != null)
			Destroy(instance);
		Camera camera = Camera.main;

		Vector3 mouse = Input.mousePosition;
		mouse = camera.ScreenToWorldPoint(mouse);

		mouse = toCartesian(mouse);
		mouse.x = Mathf.Round(mouse.x);
		mouse.y = Mathf.Round(mouse.y);
		Vector3 pos = toIsometric(new Vector3(mouse.x, mouse.y, -1f));
		instance = Instantiate(closest, pos, Quaternion.identity) as GameObject;

		// debug path finding

		if (Input.GetMouseButtonDown(0))
		{
			path = FindPath(toCartesian(spawn.transform.position), mouse);
			if (path.Count > 0)
			{
				for (int i = 0; i < tileBackup.Count; ++i)
					Destroy(tileBackup[i]);
				tileBackup = new List<GameObject>();
			}
			for (int i = 0; i < path.Count; ++i)
				tileBackup.Add(Instantiate(closest, toIsometric(path[i]), Quaternion.identity) as GameObject);
		}
	}

	private bool[,] visited;
	private int islandCount;
	private List<List<GameObject>> islands;
	private void removeSmallIslands()
	{
		islands = new List<List<GameObject>>();
		visited = new bool[column, rows];
		islandCount = 0;
		for (int x = 0; x < map.Length; ++x)
		{
			for (int y = 0; y < map[x].Length; ++y)
			{
				if (!visited[x, y] && map[x][y].layer == LayerMask.NameToLayer("Floor"))
				{
					visit(x, y);
					islandCount++;
				}
			}
		}

		for (int i = 0; i < islandCount; ++i)
		{
			if (islands[i].Count < (rows * column) / 10)
			{
				for (int a = 0; a < islands[i].Count; ++a)
					replaceTile(wall, toCartesian(islands[i][a].transform.position));
				floor -= (uint)islands[i].Count;
				islands.RemoveAt(i--);
				--islandCount;
			}
		}
	}

	private void visit(int x, int y)
	{
		if (x < 0 || x >= column || y < 0 || y >= rows)
			return;

		if (islands.Count <= islandCount)
			islands.Add(new List<GameObject>());
		if (visited[x, y] == false && map[x][y].layer == LayerMask.NameToLayer("Floor"))
		{
			islands[islandCount].Add(map[x][y]);
			visited[x, y] = true;
			visit(x - 1, y);
			visit(x, y - 1);
			visit(x + 1, y);
			visit(x, y + 1);
		}
	}

	private void connectIslands()
	{
		for (int i = 0; i < islands.Count - 1; ++i)
		{
			for (int j = i + 1; j < islands.Count; ++j)
			{
				float min = float.MaxValue;
				int firstIndex = 0;
				int secondIndex = 0;
				for (int a = 0; a < islands[i].Count; ++a)
				{
					for (int b = 0; b < islands[j].Count; ++b)
					{
						float distance = Vector3.Distance(islands[i][a].transform.position, islands[j][b].transform.position);
						if (distance < min)
						{
							min = distance;
							firstIndex = a;
							secondIndex = b;
						}
					}
				}
				islands[i][firstIndex] = replaceTile(closest, toCartesian(islands[i][firstIndex].transform.position));
				islands[j][secondIndex] = replaceTile(closest, toCartesian(islands[j][secondIndex].transform.position));
				Vector3 start = toCartesian(islands[i][firstIndex].transform.position);
				Vector3 end = toCartesian(islands[j][secondIndex].transform.position);
				start.x = Mathf.Round(start.x);
				start.y = Mathf.Round(start.y);
				end.x = Mathf.Round(end.x);
				end.y = Mathf.Round(end.y);
				float yratio = Mathf.Abs(end.y - start.y) / Mathf.Abs(end.x - start.x);
				float xmoves = float.Epsilon;
				float ymoves = float.Epsilon;
				while (start.x != end.x || start.y != end.y)
				{
					float xdir = Mathf.Clamp(end.x - start.x, -1f, 1f);
					float ydir = Mathf.Clamp(end.y - start.y, -1f, 1f);
					if (ydir != 0f && (xdir == 0f || ymoves / xmoves <= yratio))
					{
						start.y += ydir;
						++ymoves;
					}
					else
					{
						start.x += xdir;
						++xmoves;
					}
					if (start.x != end.x || start.y != end.y)
					{
						Destroy(map[(int)start.x][(int)start.y]);
						map[(int)start.x][(int)start.y] = Instantiate(floorTiles[1], toIsometric(start), Quaternion.identity) as GameObject;
					}
				}
			}
		}
	}

	private GameObject replaceTile(GameObject instance, Vector3 cartesianPosition)
	{
		int x = (int)Mathf.Round(cartesianPosition.x);
		int y = (int) Mathf.Round(cartesianPosition.y);
		Destroy(map[x][y]);
		map[x][y] = Instantiate(instance, toIsometric(new Vector3(x, y, 0f)), Quaternion.identity) as GameObject;
		return map[x][y];
	}

	class Node
	{
		public Vector3 Position { get; private set; }
		public float G { get; private set; }
		public float H { get; private set; }
		public float F { get { return this.G + this.H; } }
		public NodeState State;
		private Node parent;
		public Node ParentNode
		{
			get { return parent; }
			set
			{
				parent = value;
				G = value.G + 1f;
			}
		}

		public Node(Vector3 position, Vector3 end)
		{
			this.Position = position;
			H = Mathf.Abs(position.x - end.x) + Mathf.Abs(position.y - end.y);
			State = NodeState.Untested;
		}
	}
	private enum NodeState { Untested, Open, Closed }

	private Node[,] nodes;
	private Node endNode;
	// Find the shortest path between two cartesian coordinates.
	// Return null if no path exists.
	public List<Vector3> FindPath(Vector3 cartesianStart, Vector3 cartesianEnd)
	{
		List<Vector3> path = new List<Vector3>();
		if (cartesianStart.x < 0f || cartesianStart.x >= column || cartesianStart.y < 0f || cartesianStart.y >= rows ||
			cartesianEnd.x < 0f || cartesianEnd.x >= column || cartesianEnd.y < 0f || cartesianEnd.y >= rows)
			return path;

		nodes = new Node[column, rows];
		for (int x = 0; x < column; ++x)
		{
			for (int y = 0; y < rows; ++y)
				nodes[x, y] = new Node(new Vector3(x, y, 0f), cartesianEnd);
		}
		Node startNode = nodes[(int)cartesianStart.x, (int)cartesianStart.y];
		endNode = nodes[(int)cartesianEnd.x, (int)cartesianEnd.y];

		if (search(startNode))
		{
			Node node = endNode;
			while (node.ParentNode != null)
			{
				path.Add(node.Position);
				node = node.ParentNode;
			}
			path.Reverse();
		}
		return path;
	}

	// Find the shortest path between two isometric coordinates.
	// return null if no path exists.
	public List<Vector3> FindIsoPath(Vector3 isoStart, Vector3 isoEnd)
	{
		List<Vector3> path = FindPath(toCartesian(isoStart), toCartesian(isoEnd));
		for (int i = 0; i < path.Count; ++i)
			path[i] = toIsometric(path[i]);
		return path;
	}

	private bool search(Node current)
	{
		current.State = NodeState.Closed;
		List<Node> neighbors = getNeighbors(current);
		neighbors.Sort((first, second) => first.F.CompareTo(second.F));

		foreach (Node neighbor in neighbors)
		{
			if (neighbor.Position.x == endNode.Position.x && neighbor.Position.y == endNode.Position.y)
				return true;
		}

		Node lowest = lowestFInOpenList();
		if (lowest != null)
			return search(lowest);

		return false;
	}

	private Node lowestFInOpenList()
	{
		float lowest = float.MaxValue;
		Node node = null;
		for (int x = 0; x < column; ++x)
		{
			for (int y = 0; y < rows; ++y)
			{
				if (nodes[x,y].G < lowest && nodes[x,y].State == NodeState.Open)
				{
					node = nodes[x, y];
					lowest = node.G;
				}
			}
		}
		return node;
	}

	private List<Node> getNeighbors(Node from)
	{
		List<Node> neighbors = new List<Node>();
		Vector3[] locations = new Vector3[] {
			new Vector3(from.Position.x - 1f, from.Position.y),
			new Vector3(from.Position.x, from.Position.y - 1f),
			new Vector3(from.Position.x + 1f, from.Position.y),
			new Vector3(from.Position.x, from.Position.y + 1f)
		};



		foreach (Vector3 location in locations)
		{
			int x = (int)Mathf.Round(location.x);
			int y = (int)Mathf.Round(location.y);
			if (x < 0 || x >= nodes.GetLength(0) || y < 0 || y >= nodes.GetLength(1))
				continue;

			if (map[x][y].layer != LayerMask.NameToLayer("Floor"))
				continue;

			Node node = nodes[x, y];
			if (node.State == NodeState.Closed)
				continue;
			else if (node.State == NodeState.Open)
			{
				float gTemp = from.G + 1f;
				if (gTemp < node.G)
				{
					node.ParentNode = from;
					neighbors.Add(node);
				}
			}
			else
			{
				node.ParentNode = from;
				node.State = NodeState.Open;
				neighbors.Add(node);
			}
		}

		return neighbors;
	}

	Vector3 toIsometric(Vector3 localPosition)
	{
		float isoX = (localPosition.x - localPosition.y) * tileSizeInUnits.x / 2f;
		float isoY = (localPosition.x + localPosition.y) * tileSizeInUnits.y / 2f;
		return new Vector3(isoX, isoY, isoY);
	}

	Vector3 toCartesian(Vector3 isoPosition)
	{
		float cartX = (isoPosition.x * (2 / tileSizeInUnits.x) + isoPosition.y * (2 / tileSizeInUnits.y)) / 2;
		float cartY = isoPosition.y * (2 / tileSizeInUnits.y) - cartX;
		return new Vector3(cartX, cartY, isoPosition.z);
	}
}
