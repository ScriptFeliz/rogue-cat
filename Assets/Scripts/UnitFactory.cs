using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitFactoryType { Undefined, Player, Enemy, Exit };

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
        {
            gameObjDestroy(gameObjList[0]);
        }
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
            return false;
        if (!gameObjList.Remove(gameObj))
            return false;
        Destroy(gameObj);
        return true;
    }

    static public GameObject instantiate(UnitFactoryType type, Cart spawnPos)
    {
        GameObject gameObj = null;

        switch (type)
        {
            case UnitFactoryType.Enemy:
                Enemy enemy = createEnemy(spawnPos);
                gameObj = enemy.gameObject;
                break;
            case UnitFactoryType.Exit:
                gameObj = createExit(spawnPos);
                break;
            default:
                Debug.LogError("Instantiate a gameObject of type " + type.ToString() + " with UnitFactory.instantiate() is forbidden");
                break;
        }

        return gameObj;
    }
    static public void destroy(GameObject gameObj, UnitFactoryType type)
    {
        switch (type)
        {
            case UnitFactoryType.Enemy:
                destroyEnemy(gameObj);
                break;
            case UnitFactoryType.Exit:
                Destroy(gameObj);
                break;
            default:
                Debug.LogError("destroy a gameObject of type " + type.ToString() + " with UnitFactory.destroy() is forbidden. gameObj tag: " + gameObj.transform.tag);
                break;
        }
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
