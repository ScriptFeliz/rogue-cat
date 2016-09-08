using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{

	public int column = 20;
	public int rows = 20;
	public GameObject[] floorTiles;
	public GameObject wall;
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
					instance = Instantiate(floorTiles[Random.Range(0, floorTiles.Length)], toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
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

		if (floor < column * rows / 3f)
			MapSetup(level);

		connectIslands();
	}

	GameObject instance;
	private void Update()
	{
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
				{
					Vector3 cartPos = toCartesian(islands[i][a].transform.position);
					cartPos.x = Mathf.Round(cartPos.x);
					cartPos.y = Mathf.Round(cartPos.y);
					map[(int)cartPos.x][(int)cartPos.y] = Instantiate(wall, toIsometric(new Vector3(cartPos.x, cartPos.y, 0)), Quaternion.identity) as GameObject;
					Destroy(islands[i][a]);
				}
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
		for (int i = 0; i < islands.Count; ++i)
		{

		}
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
