using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {

	public int column = 20;
	public int rows = 20;
	public GameObject[] floorTiles;
	public GameObject wall;
	public Vector3 tileSizeInUnits = new Vector3(1.0f, 0.57f, 0.5f);
	public float perlinOffset = 0.38f;
	[Range(0,1)]
	public float perlinRange = 0.11f;

	private GameObject[][] map = null;
	private uint floor;

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
		for (int y = 0;  y < rows; y++)
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
	}

	bool[,] visited;
	int islandCount;
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
				if (!visited[x,y] && map[x][y].layer == LayerMask.NameToLayer("Floor"))
				{
					visit(x, y);
					islandCount++;
				}
			}
		}
		Debug.Log("ISLAND COUNT: "+islandCount);
		
		for (int i = 0; i < islandCount; ++i)
		{
			Debug.Log(islands[i].Count + " : " + rows * column / 10);
			if (islands[i].Count < (rows * column) / 10)
			{
				Debug.Log("ALO");
				for (int a = 0; a < islands[i].Count; ++a)
					Destroy(islands[i][a]);
				floor -= (uint)islands[i].Count;
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

	Vector3 toIsometric(Vector3 localPosition)
	{
		float isoX = (localPosition.x - localPosition.y) * tileSizeInUnits.x / 2f;
		float isoY = (localPosition.x + localPosition.y) * tileSizeInUnits.y / 2f;
		return new Vector3(isoX, isoY, 1f);
	}
}
