using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
	public uint columnCount;
	public uint rowCount;
    public uint enemyCount;

    private uint columnInit;
    private uint rowInit;
    private uint enemyInit;
    public uint getEnemyCount() { return enemyCount; }

	public GameObject[] floorTiles;
	public TileMap[] tilemaps;
	public GameObject[] walls;
    public GameObject spawn;
	[Range(0f, 0.5f)]
	public float perlinOffset = 0.38f;
	[Range(0, 1)]
	public float perlinRange = 0.11f;
    public int viewDistance = 5;

	public Cube[][] map = null;
	private uint floor;

    //debug
    public bool fogOfWar = true;
	public GameObject closest;

    public void initOnce()
    {
        columnInit = columnCount;
        rowInit = rowCount;
        enemyInit = enemyCount;
    }

    public void litMap(Cart position)
    {
        if (position == null)
            return;
        if (!fogOfWar)
            return;

        int x = position.x;
        int y = position.y;

        if (!map[x][y].litVisited)
        {
            for (int i = 0; i < columnCount; ++i)
            {
                for (int j = 0; j < rowCount; ++j)
                {
                    Cart cartPos = map[i][j].cartPos;
                    SpriteRenderer cubeRenderer = map[cartPos.x][cartPos.y].instance.GetComponent<SpriteRenderer>();
                    float distance = (cartPos - position).magnitude;

                    // fogWar
                    if (distance <= viewDistance + 2)
                    {
                        if (distance <= viewDistance)
                        {
                            if (map[i][j].unit != null)
                                map[i][j].unit.GetComponent<SpriteRenderer>().enabled = true;
                            cubeRenderer.material.color = Color.white;
                        }
                        else
                        {
                            cubeRenderer.material.color = Color.gray;
                        }
                    }
                    // Unit
                    else
                    {
                        if (map[i][j].unit != null)
                            map[i][j].unit.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }
    }

	public void MapSetup(uint level)
	{
        // 
        columnCount = columnInit + level * 2;
        rowCount = rowInit + level * 2;
        enemyCount = enemyInit + level / 2;

        //
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
				for (int y = 0; y < map[x].Length; ++y)
					Destroy(map[x][y].instance);
		}

		// allocate map array
		map = new Cube[columnCount][];
		for (int i = 0; i < columnCount; ++i)
			map[i] = new Cube[rowCount];

		// generate map with using perlin noise
		for (int y = 0; y < rowCount; y++)
		{
			for (int x = 0; x < columnCount; x++)
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
					instance = Instantiate(tilemaps[index].GetTile(UnityEngine.Random.Range(0f, 100f)), new Cart(x, y).toIsometric(), Quaternion.identity) as GameObject;
					++floor;
				}
				else
				{
					instance = Instantiate(walls[0], new Cart(x, y).toIsometric(), Quaternion.identity) as GameObject;
				}
                map[x][y] = new Cube(instance, new Cart(x, y));
			}
		}

		removeSmallIslands();

		// if the map is too small, regenerate it
		if (floor < columnCount * rowCount / 3f)
			MapSetup(level); // !StackOverflow

		connectIslands();

        for (int x = 0; x < map.Length; ++x)
        {
            for (int y = 0; y < map[x].Length; ++y)
            {
                if (isWall(x, y))
                {
                    if (isWall(x - 1, y - 1))
                    {
                        Vector3 newPos = map[x][y].instance.transform.position;
                        newPos.z = map[x - 1][y - 1].instance.transform.position.z - 0.01f;
                        map[x][y].instance.transform.position = newPos;
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
                    if (map[x][y].instance.transform.position.z > map[x - 1][y].instance.transform.position.z && map[x][y].instance.transform.position.z > map[x][y - 1].instance.transform.position.z)
                    {
                        if ((!isWall(x, y - 1) || map[x][y - 1].instance.name != walls[1].name + "(Clone)") && (!isWall(x, y + 1) || map[x][y].instance.transform.position.z < map[x][y + 1].instance.transform.position.z))
                            newTile = walls[1];
                        else if ((!isWall(x - 1, y) || map[x - 1][y].instance.name != walls[2].name + "(Clone)") && (!isWall(x + 1, y) || map[x][y].instance.transform.position.z < map[x + 1][y].instance.transform.position.z))
                            newTile = walls[2];
                    }
                    if (newTile != null)
                    {
                        Destroy(map[x][y].instance);
                        map[x][y].instance = Instantiate(newTile, map[x][y].instance.transform.position, Quaternion.identity) as GameObject;
                    }
                }
                if (fogOfWar)
                {
                    SpriteRenderer renderer = map[x][y].instance.GetComponent<SpriteRenderer>();
                    renderer.material.color = Color.black;
                }
            }
        }
	}

    enum Border { Undefined, Left, Right };
    public Cart spawnPlayer()
    {
        Cart spawnPos = new Cart();

        Border border = UnityEngine.Random.Range(0f, 1f) < 0.5 ? Border.Left : Border.Right;
        int yy = 1;
        int trigger = 0;
        while (true)
        {
            spawnPos.x = (int)UnityEngine.Random.Range(0f, (float)map.Length - 1);
            spawnPos.y = border == Border.Left ? 0 + yy : map[spawnPos.x].Length - yy;

            int x = spawnPos.x;
            int y = spawnPos.y;

            if (map[x][y].instance.layer == LayerMask.NameToLayer("Floor"))
                break;

            trigger++;
            if (trigger > 20)
            {
                yy++;
                trigger = 0;
            }
        }

        // spawn exit
        border = border == Border.Left ? Border.Right : Border.Left;
        trigger = 0;
        yy = 1;
        Cart exitPos = new Cart();
        while (true)
        {
            exitPos.x = (int)Math.Round(UnityEngine.Random.Range(0f, (float)map.Length - 1));
            exitPos.y = border == Border.Left ? 0 + yy : map[exitPos.x].Length - yy;

            int x = exitPos.x;
            int y = exitPos.y;

            if (map[x][y].instance.layer == LayerMask.NameToLayer("Floor"))
                break;

            trigger++;
            if (trigger > 20)
            {
                yy++;
                trigger = 0;
            }
        }
        map[exitPos.x][exitPos.y].unit = UnitFactory.createExit(new Cart(exitPos.x, exitPos.y));

        return spawnPos;
    }

    public Cart spawnEnemy()
    {
        Cart spawnPos = new Cart();

        int trigger = 0;
        while (true)
        {
            spawnPos.x = (int)UnityEngine.Random.Range(0f, (float)map.Length - 1);
            spawnPos.y = (int)UnityEngine.Random.Range(0f, (float)map[spawnPos.x].Length - 1);

            int x = (int)spawnPos.x;
            int y = (int)spawnPos.y;

            if (map[x][y].instance.layer == LayerMask.NameToLayer("Floor") && map[x][y].unit == null)
                break;

            trigger++;
            if (trigger > 50)
            {
                Debug.LogError("Trigger " + trigger);
                break;
            }
        }

        return spawnPos;
    }


    private bool isWall(int x, int y)
    {
        if (x < 0 || x >= columnCount || y < 0 || y >= rowCount)
            return false;
        return map[x][y].instance.layer == LayerMask.NameToLayer("BlockingLayer");
    }

	GameObject instance;
	List<Cart> path = new List<Cart>();
	List<GameObject> tileBackup = new List<GameObject>();
	private void Update()
	{
		// debug mouse location
		if (instance != null)
			Destroy(instance);
		Camera camera = Camera.main;

		Vector3 isoMouse = Input.mousePosition;
		isoMouse = camera.ScreenToWorldPoint(isoMouse);

		Cart cartMouse = Utils.toCartesian(isoMouse);
		instance = Instantiate(closest, cartMouse.toIsometric(), Quaternion.identity) as GameObject;

		// debug path finding

		if (Input.GetMouseButtonDown(0))
		{
            Player player = GameManager.instance.player;
			path = findPath(player.position, cartMouse);
			if (path.Count > 0)
			{
				for (int i = 0; i < tileBackup.Count; ++i)
					Destroy(tileBackup[i]);
				tileBackup = new List<GameObject>();
			}
			for (int i = 0; i < path.Count; ++i)
				tileBackup.Add(Instantiate(closest, path[i].toIsometric(), Quaternion.identity) as GameObject);
		}
	}

	private bool[,] visited;
	private int islandCount;
	private List<List<Cube>> islands;
	private void removeSmallIslands()
	{
		islands = new List<List<Cube>>();
		visited = new bool[columnCount, rowCount];
		islandCount = 0;
		for (int x = 0; x < map.Length; ++x)
		{
			for (int y = 0; y < map[x].Length; ++y)
			{
				if (!visited[x, y] && map[x][y].instance.layer == LayerMask.NameToLayer("Floor"))
				{
					visit(x, y);
					islandCount++;
				}
			}
		}

		for (int i = 0; i < islandCount; ++i)
		{
			if (islands[i].Count < (rowCount * columnCount) / 10)
			{
				for (int j = 0; j < islands[i].Count; ++j)
					replaceTile(walls[0], islands[i][j].cartPos);
				floor -= (uint)islands[i].Count;
				islands.RemoveAt(i--);
				--islandCount;
			}
		}
	}

	private void visit(int x, int y)
	{
		if (x < 0 || x >= columnCount || y < 0 || y >= rowCount)
			return;

		if (islands.Count <= islandCount)
			islands.Add(new List<Cube>());
		if (visited[x, y] == false && map[x][y].instance.layer == LayerMask.NameToLayer("Floor"))
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
                        float distance = islands[i][a].cartPos.distanceTo(islands[j][b].cartPos);
						if (distance < min)
						{
							min = distance;
							firstIndex = a;
							secondIndex = b;
						}
					}
				}
				Cart start = islands[i][firstIndex].cartPos;
				Cart end = islands[j][secondIndex].cartPos;

                int dy = Mathf.Abs(end.y - start.y);
                int dx = Mathf.Abs(end.x - start.x);
                dy = dy == 0 ? 1 : dy;
                dx = dx == 0 ? 1 : dx;
				int yratio = dy / dx;

				int xmoves = 1;
				int ymoves = 1;
				while (start.x != end.x || start.y != end.y)
				{
					int xdir = (int)Mathf.Clamp(end.x - start.x, -1f, 1f);
					int ydir = (int)Mathf.Clamp(end.y - start.y, -1f, 1f);
					if (ydir != 0 && (xdir == 0 || ymoves / xmoves <= yratio))
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
						Destroy(map[start.x][start.y].instance);
						map[start.x][start.y].instance = Instantiate(floorTiles[1], start.toIsometric(), Quaternion.identity) as GameObject;
					}
				}
			}
		}
	}

	private void replaceTile(GameObject instance, Cart cartesianPosition)
	{
		int x = cartesianPosition.x;
		int y = cartesianPosition.y;
		Destroy(map[x][y].instance);
		map[x][y].instance = Instantiate(instance, cartesianPosition.toIsometric(), Quaternion.identity) as GameObject;
	}

	class Node
	{
		public Cart position { get; private set; }
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
                if (parent.position.x != this.position.x && parent.position.y != this.position.y)
                    g = value.g + 1.32f;
                else
                    g = value.g + 1f;
			}
		}

		public Node(Cart position, Cart end)
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
	public List<Cart> findPath(Cart cartesianStart, Cart cartesianEnd)
	{
		List<Cart> path = new List<Cart>();
		if (cartesianStart.x < 0 || cartesianStart.x >= columnCount || cartesianStart.y < 0 || cartesianStart.y >= rowCount ||
			cartesianEnd.x < 0 || cartesianEnd.x >= columnCount || cartesianEnd.y < 0 || cartesianEnd.y >= rowCount)
			return path;

		nodes = new Node[columnCount, rowCount];
		for (int x = 0; x < columnCount; ++x)
		{
			for (int y = 0; y < rowCount; ++y)
				nodes[x, y] = new Node(new Cart(x, y), cartesianEnd);
		}
		Node startNode = nodes[cartesianStart.x, cartesianStart.y];
		endNode = nodes[cartesianEnd.x, cartesianEnd.y];

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
		return path;
	}

	// Find the shortest path between two isometric coordinates using A* algorithm.
	// return an empty list if no path exists.
	public List<Vector3> findIsoPath(Vector3 isoStart, Vector3 isoEnd)
	{
		List<Cart> cartPath = findPath(Utils.toCartesian(isoStart), Utils.toCartesian(isoEnd));
        List<Vector3> isoPath = new List<Vector3>();

        for (int i = 0; i < cartPath.Count; ++i)
            isoPath.Add(cartPath[i].toIsometric());
		return isoPath;
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

			if (map[x][y].instance.layer != LayerMask.NameToLayer("Floor"))
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
