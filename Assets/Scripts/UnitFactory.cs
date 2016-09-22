using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitState { Hidden, Shown };

public class UnitFactory : MonoBehaviour {

    static private GameObject playerPrefab;
    static private GameObject enemyPrefab;
    static private GameObject exitPrefab;

    static private float speed;

    static private List<GameObject> gameObjList = new List<GameObject>();

    static public List<Enemy> enemyList = new List<Enemy>();

    static public void reset()
    {
        playerPrefab = GameManager.instance.playerPrefab;
        enemyPrefab = GameManager.instance.enemyPrefab;
        exitPrefab = GameManager.instance.exitPrefab;

        speed = GameManager.instance.speed;

        while (gameObjList.Count > 0)
            gameObjDestroy(gameObjList[0]);
        enemyList.Clear();
    }

    // gameObject
    static private GameObject gameObjCreate(GameObject prefab, Cart spawnPos)
    {
        GameObject gameObj = Instantiate(prefab, spawnPos.toIsometric(), Quaternion.identity) as GameObject;
        if (gameObj == null)
            return null;
        gameObjList.Add(gameObj);
        return gameObj;
    }
    static private bool gameObjDestroy(GameObject gameObj)
    {
        if (gameObj == null)
        {
            Debug.LogError("UnitFactory: gameObjDestroy() gameObj null");
            return false;
        }
        Destroy(gameObj);
        if (!gameObjList.Remove(gameObj))
        {
            Debug.LogError("UnitFactory: gameObjDestroy() gameObjList.remove() failed");
            return false;
        }
        return true;
    }

    // player
    static public Player createPlayer()
    {
        Cart spawnPos = GameManager.instance.mapManager.spawnPlayer();

        GameObject instance = gameObjCreate(playerPrefab, spawnPos);

        Player player = instance.GetComponent<Player>();
        player.initialize(spawnPos, 100, 15, speed);

        return player;
    }

    // enemy
    static public Enemy createEnemy(Cart spawnPos)
    {
        GameObject instance = gameObjCreate(enemyPrefab, spawnPos);

        Enemy enemy = instance.GetComponent<Enemy>();
        enemy.initialize(spawnPos, 15, 5, speed);

        enemyList.Add(enemy);

        return enemy;
    }
    static public void destroyEnemy(GameObject gameObj)
    {
        Enemy enemy = gameObj.GetComponent<Enemy>();
        enemyList.Remove(enemy);

        gameObjDestroy(gameObj);
    }

    // exit
    static public GameObject createExit(Cart spawnPos)
    {
        return gameObjCreate(exitPrefab, spawnPos);
    }
}
