using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {

	public int column = 20;
	public int rows = 20;
	public GameObject[] floorTiles;
	public GameObject wall;
	public Vector3 tileSizeInUnits = new Vector3(1.0f, 0.57f, 0.5f);
	private List<GameObject> floor = new List<GameObject>();
	public float perlinOffset = 0.38f;
	[Range(0,1)]
	public float perlinRange = 0.11f;
	public void MapSetup(uint level)
	{
		for (int i = 0; i < floor.Count; i++)
		{
			Destroy(floor[i]);
		}
		floor.Clear();
		for (int y = 0;  y < column; y++)
		{
			for (int x = 0; x < rows; x++)
			{
				float seed = (float)Network.time + 0.1f;
				float p = Mathf.PerlinNoise((float)x * perlinRange + seed, (float)y * perlinRange + seed);
				Debug.Log(p);
				if (p > perlinOffset)
				{
					GameObject instance = Instantiate(floorTiles[Random.Range(0, floorTiles.Length)], toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
					floor.Add(instance);
				}
				else
				{
					GameObject instance = Instantiate(wall, toIsometric(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
					floor.Add(instance);
				}
			}
		}
	}

	Vector3 toIsometric(Vector3 localPosition)
	{
		float isoX = (localPosition.x - localPosition.y) * tileSizeInUnits.x / 2f;
		float isoY = (localPosition.x + localPosition.y) * tileSizeInUnits.y / 2f;
		return new Vector3(isoX, isoY, 1f);	
	}
}
