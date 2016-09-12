using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	public uint level = 1;
	public bool cameraFree = true;
	[System.NonSerialized]
	public MapManager mapManager;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);

		DontDestroyOnLoad(gameObject);
		mapManager = GetComponent<MapManager>();
		mapManager.MapSetup(level);
	}

	private void Update()
	{
		if (Input.GetKeyDown("space"))
			mapManager.MapSetup(level);
	}
}
