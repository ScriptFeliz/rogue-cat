using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{

	public int column = 20;
	public int rows = 20;
	public GameObject[] floorTiles;
	public TileMap[] tilemaps;
	public GameObject[] walls;
    public GameObject spawn;
	[Range(0f, 0.5f)]
	public float perlinOffset = 0.38f;
	[Range(0, 1)]
	public float perlinRange = 0.11f;
    public int viewDistance = 5;

	private GameObject[][] map = null;
	private uint floor;

    //debug
    public bool fogOfWar = true;
	public GameObject closest;

    private bool[][] litMapVisited;
    public void litMap(Vector3 position)
    {
        if (!fogOfWar)
            return;

        int x = (int)position.x;
        int y = (int)position.y;

        if (!litMapVisited[x][y])
        {
            for (int i = 0; i < column; ++i)
            {
                for (int j = 0; j < rows; ++j)
                {
                    Vector3 cartPos = Utils.toCartesian(map[i][j].transform.position);
                    int xx = (int)Mathf.Round(cartPos.x);
                    int yy = (int)Mathf.Round(cartPos.y);
                        SpriteRenderer renderer = map[xx][yy].GetComponent<SpriteRenderer>();
                    float distance = (cartPos - position).magnitude;
                    if (distance <= viewDistance)
                    {
                        renderer.material.color = Color.white;
                    }
                    else if (distance <= viewDistance + 2)
                    {
                        renderer.material.color = Color.gray;
                    }
                }
            }
        }
    }

	public void MapSetup(uint level)
	{
		floor = 0;
		// remove debug pathfinding tiles
		foreach (GameObject tile in tileBackup)
		{
			Destroy(tile);
		}
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
        litMapVisited = new bool[column][];
		for (int i = 0; i < column; ++i)
		{
			map[i] = new GameObject[rows];
            litMapVisited[i] = new bool[rows];
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
					int index = 0;
					for (int i = 0; i < tilemaps.Length; ++i)
					{
						if (p < perlinOffset + perlinOffset / (tilemaps.Length - i))
						{
							index = i;
							break;
						}
					}
					if (p < perlinOffset + perlinOffset / 2f)
						index = 0;
					instance = Instantiate(tilemaps[index].GetTile(UnityEngine.Random.Range(0f, 100f)), Utils.toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
					++floor;
				}
				else
				{
					instance = Instantiate(walls[0], Utils.toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
				}
				map[x][y] = instance;
			}
		}

		removeSmallIslands();

		// if the map is too small, regenerate it
		if (floor < column * rows / 3f)
			MapSetup(level);

		connectIslands();

        // Setup spawn and buildings
		bool found = false;
		for (int x = 0; x < map.Length; ++x)
		{
			for (int y = 0; y < map[x].Length; ++y)
			{
				if (!found && map[x][y].layer == LayerMask.NameToLayer("Floor"))
				{
					Destroy(map[x][y]);
					map[x][y] = Instantiate(spawn, Utils.toIsometric(new Vector3(x, y, 0f)), Quaternion.identity) as GameObject;
					found = true;
					spawn = map[x][y];
					break;
				}
                else if (isWall(x,y))
                {
                    if (isWall(x-1,y-1))
                    {
                        Vector3 newPos = map[x][y].transform.position;
                        newPos.z = map[x-1][y-1].transform.position.z - 0.01f;
                        map[x][y].transform.position = newPos;
                    }
                }
			}
		}

        // Update walls sprite
        for (int x = 0; x < map.Length; ++x)
        {
            for (int y = 0; y < map[x].Length; ++y)
            {
                GameObject newTile = null;
                if (isWall(x,y) && isWall(x-2,y-2))
                {
                    if (map[x][y].transform.position.z > map[x - 1][y].transform.position.z && map[x][y].transform.position.z > map[x][y - 1].transform.position.z)
                    {
                        if ((!isWall(x, y - 1) || map[x][y - 1].name != walls[1].name + "(Clone)") && (!isWall(x, y + 1) || map[x][y].transform.position.z < map[x][y + 1].transform.position.z))
                            newTile = walls[1];
                        else if ((!isWall(x - 1, y) || map[x - 1][y].name != walls[2].name + "(Clone)") && (!isWall(x + 1, y) || map[x][y].transform.position.z < map[x + 1][y].transform.position.z))
                            newTile = walls[2];
                    }
                    if (newTile != null)
                    {
                        Destroy(map[x][y]);
                        map[x][y] = Instantiate(newTile, map[x][y].transform.position, Quaternion.identity) as GameObject;
                    }
                }
                if (fogOfWar)
                {
                    SpriteRenderer renderer = map[x][y].GetComponent<SpriteRenderer>();
                    renderer.material.color = Color.black;
                }
            }
        }
	}

    private bool isWall(int x, int y)
    {
        if (x < 0 || x >= column || y < 0 || y >= rows)
            return false;
        return map[x][y].layer == LayerMask.NameToLayer("BlockingLayer");
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

		mouse = Utils.toCartesian(mouse);
		mouse.x = Mathf.Round(mouse.x);
		mouse.y = Mathf.Round(mouse.y);
		Vector3 pos = Utils.toIsometric(new Vector3(mouse.x, mouse.y, -1f));
		instance = Instantiate(closest, pos, Quaternion.identity) as GameObject;

		// debug path finding

		if (Input.GetMouseButtonDown(0))
		{
            if (path == null)
                return;
			path = findPath(Utils.toCartesian(spawn.transform.position), mouse);
			if (path.Count > 0)
			{
				for (int i = 0; i < tileBackup.Count; ++i)
					Destroy(tileBackup[i]);
				tileBackup = new List<GameObject>();
			}
			for (int i = 0; i < path.Count; ++i)
				tileBackup.Add(Instantiate(closest, Utils.toIsometric(path[i]), Quaternion.identity) as GameObject);
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
					replaceTile(walls[0], Utils.toCartesian(islands[i][a].transform.position));
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
				Vector3 start = Utils.toCartesian(islands[i][firstIndex].transform.position);
				Vector3 end = Utils.toCartesian(islands[j][secondIndex].transform.position);
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
						map[(int)start.x][(int)start.y] = Instantiate(floorTiles[1], Utils.toIsometric(start), Quaternion.identity) as GameObject;
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
		map[x][y] = Instantiate(instance, Utils.toIsometric(new Vector3(x, y, 0f)), Quaternion.identity) as GameObject;
		return map[x][y];
	}

	class Node
	{
		public Vector3 position { get; private set; }
		public float g { get; private set; } // cost from startNode to this node
		public float h { get; private set; } // estimated cost from endNode to this node
		public float f { get { return this.g + this.h; } } // sum of g and h
		private NodeState s;
		public NodeState state
		{
			get { return s; }
			set
			{
				s = value;
				if (value == NodeState.Open)
					GameManager.instance.mapManager.openList.Add(this);
				else if (value == NodeState.Closed)
				{
					List<Node> openList = GameManager.instance.mapManager.openList;
					for (int i = 0; i < openList.Count; ++i)
					{
						if (openList[i].position == position)
							openList.RemoveAt(i);
					}
				}
			}
		}
		private Node parent;
		public Node parentNode
		{
			get { return parent; }
			set
			{
				parent = value;
				g = value.g + 1f;
			}
		}

		public Node(Vector3 position, Vector3 end)
		{
			this.position = position;
			h = Mathf.Abs(position.x - end.x) + Mathf.Abs(position.y - end.y);
			s = NodeState.Untested;
		}
	}
	private enum NodeState { Untested, Open, Closed }

	private Node[,] nodes;
	private Node endNode;
	private List<Node> openList;
	// Find the shortest path between two cartesian coordinates using A* algorithm.
	// Return an empty list if no path exists.
	public List<Vector3> findPath(Vector3 cartesianStart, Vector3 cartesianEnd)
	{
		List<Vector3> path = new List<Vector3>();
		if (cartesianStart.x < 0f || cartesianStart.x >= column || cartesianStart.y < 0f || cartesianStart.y >= rows ||
			cartesianEnd.x < 0f || cartesianEnd.x >= column || cartesianEnd.y < 0f || cartesianEnd.y >= rows)
			return null;

		nodes = new Node[column, rows];
		for (int x = 0; x < column; ++x)
		{
			for (int y = 0; y < rows; ++y)
				nodes[x, y] = new Node(new Vector3(x, y, 0f), cartesianEnd);
		}
		Node startNode = nodes[(int)cartesianStart.x, (int)cartesianStart.y];
		endNode = nodes[(int)cartesianEnd.x, (int)cartesianEnd.y];

		openList = new List<Node>();

		if (search(startNode))
		{
			Node node = endNode;
			while (node.parentNode != null)
			{
				path.Add(node.position);
				node = node.parentNode;
			}
			path.Reverse();
		}
		return path.Count == 0 ? null : path;
	}

	// Find the shortest path between two isometric coordinates using A* algorithm.
	// return an empty list if no path exists.
	public List<Vector3> findIsoPath(Vector3 isoStart, Vector3 isoEnd)
	{
		List<Vector3> path = findPath(Utils.toCartesian(isoStart), Utils.toCartesian(isoEnd));
        if (path == null)
            return null;

        for (int i = 0; i < path.Count; ++i)
            path[i] = Utils.toIsometric(path[i]);
		return path;
	}

	private bool search(Node current)
	{
		while (current  != null)
		{
			current.state = NodeState.Closed;
			List<Node> neighbors = getNeighbors(current);

			foreach (Node neighbor in neighbors)
			{
				if (neighbor.position == endNode.position)
					return true;
			}

			current = lowestFInOpenList();
		}

		return false;
	}

	private Node lowestFInOpenList()
	{
		float lowest = float.MaxValue;
		List<Node> lowestNodeList = new List<Node>();

		foreach (Node node in openList)
		{
			if (node.g < lowest)
			{
				lowestNodeList = new List<Node>();
				lowestNodeList.Add(node);
				lowest = node.g;
			}
			else if (node.g == lowest)
				lowestNodeList.Add(node);
		}

		if (lowestNodeList.Count == 0)
			return null;
		else
			return lowestNodeList[0];
	}

	private List<Node> getNeighbors(Node from)
	{
		List<Node> neighbors = new List<Node>();
		Vector3[] locations = new Vector3[] {
			new Vector3(from.position.x - 1f, from.position.y),
			new Vector3(from.position.x, from.position.y - 1f),
			new Vector3(from.position.x + 1f, from.position.y),
			new Vector3(from.position.x, from.position.y + 1f),
			new Vector3(from.position.x + 1f, from.position.y + 1f),
			new Vector3(from.position.x - 1f, from.position.y - 1f),
			new Vector3(from.position.x - 1f, from.position.y + 1f),
			new Vector3(from.position.x + 1f, from.position.y - 1f),
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
			if (node.state == NodeState.Closed)
				continue;
			else if (node.state == NodeState.Open)
			{
				float gTemp = from.g + 1f;
				if (gTemp < node.g)
				{
					node.parentNode = from;
					neighbors.Add(node);
				}
			}
			else
			{
				node.parentNode = from;
				node.state = NodeState.Open;
				neighbors.Add(node);
			}
		}

		return neighbors;
	}
}
