using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	public uint level = 1;
	public bool cameraFree = true;

	private void Awake()
	{

		if (instance == null)
			instance = this;
		else
			Destroy(this);

		DontDestroyOnLoad(gameObject);
		MapManager mapManager = GetComponent<MapManager>();
		mapManager.MapSetup(level);
	}

	private void Update()
	{
		if (Input.GetKeyDown("space"))
		{
			MapManager mapManager = GetComponent<MapManager>();
			mapManager.MapSetup(level);
		}
	}
}
