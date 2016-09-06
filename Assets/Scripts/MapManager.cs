using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {

	public int column = 20;
	public int rows = 20;
	public GameObject floorTile;
	public Vector3 tileSizeInUnits = new Vector3(1.0f, 0.57f, 0.5f);
	private List<GameObject> floor = new List<GameObject>();

	public void MapSetup()
	{
		for(int i = 0; i < floor.Count; i++)
		{
			Destroy(floor[i]);
		}

		floor.Clear();
		for (int y = 0;  y < column; y++)
		{
			for (int x = 0; x < rows; x++)
			{
				GameObject instance = Instantiate(floorTile, Snap(new Vector3(x, y, 0)), Quaternion.identity) as GameObject;
				floor.Add(instance);
			}
		}
	}

	Vector3 Snap(Vector3 localPosition)
	{
		float isoX = (localPosition.x - localPosition.y) * tileSizeInUnits.y;
		float isoY = (localPosition.x + localPosition.y) * tileSizeInUnits.y / 2f;
		return new Vector3(isoX, isoY, 1f);	
	}
}
