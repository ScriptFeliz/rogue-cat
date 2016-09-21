using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
    [System.NonSerialized]
	public MapManager mapManager;
	public uint level = 1;
	public bool cameraFree = true;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject exitPrefab;

    public Player player;
    public List<Enemy> enemyList = new List<Enemy>();

    private int movingUnitDoneCount;

    public int turnLeft;

    private bool reset;
    public float speed = 5f;


	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
		DontDestroyOnLoad(gameObject);
        init();
	}

    public void enemySkipMove()
    {
        Cart targetPos;
        foreach (Enemy enemy in enemyList)
            if (enemy.readyToAttack(out targetPos))
                enemy.skipMoveCount = 4;
    }

    private void init()
    {
        // map
        mapManager = GetComponent<MapManager>();
        mapManager.MapSetup(level);

        // reset UnitFactory
        UnitFactory.reset();

        // player
        player = UnitFactory.createPlayer();

        // Camera
        Camera camera = Camera.main;
        camera.transform.position = player.gameObject.transform.position - new Vector3(0, 0, 30);

        // enemy
        for (int i = 0; i < 8; i ++)
        {
            Cart enemySpawn = instance.mapManager.spawnEnemy();
            instance.mapManager.map[enemySpawn.x][enemySpawn.y].isTaken = true;
            instance.mapManager.map[enemySpawn.x][enemySpawn.y].unitType = UnitFactoryType.Enemy;
            // Enemy will be spawned in LitMap()
        }

        movingUnitDoneCount = enemyList.Count + 1; // + 1 for player
        turnLeft = 0;
        reset = false;
    }

    public void movingUnitIsDone()
    {
        movingUnitDoneCount++;
    }

	private void Update()
	{
        //Debug.Log("MovingUnitList.Count " + movingUnitList.Count + " movingUnitDoneCount " + movingUnitDoneCount);
        if (movingUnitDoneCount != enemyList.Count + 1)
            return;

        // You better wait for all move() Coroutines to be done before you reset the game
        if (Input.GetKeyDown("space") || reset)
        {
            init();
            return;
        }

        if (turnLeft <= 0)
            return;
        turnLeft--;

        movingUnitDoneCount = 0;
        player.attemptMove();

        enemyList = new List<Enemy>(UnitFactory.enemyList);
        foreach (Enemy enemy in enemyList)
            enemy.attemptMove();
	}

    public void win()
    {
        Debug.Log("You Win!");
        reset = true;
    }

    public void gameOver()
    {
        Debug.Log("Game Over");
        reset = true;
    }
}
