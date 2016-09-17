using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
    [System.NonSerialized]
	public MapManager mapManager;
    public GameObject playerPrefab;
	public uint level = 1;
	public bool cameraFree = true;

    private GameObject player;
    private Player playerScript;
    private enum Turn { Undefined, Player, Enemy };
    private Turn turn;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
		DontDestroyOnLoad(gameObject);
        init();
	}

    private void init()
    {
		mapManager = GetComponent<MapManager>();
		mapManager.MapSetup(level);
        if (player != null)
            Destroy(player);
        player = Instantiate(playerPrefab) as GameObject;
        turn = Turn.Player;
        playerScript = player.GetComponent<Player>();
        playerScript.init();
    }

	private void Update()
	{
        if (Input.GetKeyDown("space"))
            init();
        switch (turn)
        {
            case Turn.Player:
                if (!playerScript.moving)
                    playerScript.attemptMove();
                break;
            default:
                Debug.Log("Yolo");
                break;
        }
	}
}
