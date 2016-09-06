using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;

	private void Awake()
	{

		if (instance == null)
			instance = this;
		else
			Destroy(this);

		DontDestroyOnLoad(gameObject);
		MapManager mapManager = GetComponent<MapManager>();
		mapManager.MapSetup();
	}

	private void Update()
	{
		MapManager mapManager = GetComponent<MapManager>();
		mapManager.MapSetup();
	}
}
