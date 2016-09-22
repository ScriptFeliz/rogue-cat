using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MovingUnit {

    public void initialize(Cart startPos, int health, int damage, float speed)
    {
        base.movingUnitInitialize(startPos, health, damage, speed);
    }

	override protected void Update()
    {
        Camera camera = Camera.main;

        GameManager.instance.mapManager.litMap(position);
	    if (Input.GetMouseButtonDown(0))
        {
            Cart dest = Utils.toCartesian(camera.ScreenToWorldPoint(Input.mousePosition));
            computePath(dest);
            GameManager.instance.turnLeft = path.Count;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Stop Enemies");
            GameManager.instance.enemySkipMove();
        }

        base.Update();
	}

	private IEnumerator loot(GameObject item)
	{
		while (moving)
			yield return null;
		GameManager.instance.itemManager.loot(item);
	}

    override public bool attemptMove()
    {
		GameObject unit, item;
        bool canMove = move(out unit, out item);
        if (!canMove && unit != null)
        {
            if (unit.transform.tag == "Enemy")
            {
                path.Clear();
                GameManager.instance.turnLeft = 0;
            }
            if (unit.transform.tag == "Finish")
            {
                GameManager.instance.win();
            }
        }
		if (canMove && item != null)
		{
			GameManager.instance.mapManager.map[nextPos.x][nextPos.y].item = null;
			StartCoroutine(loot(item));
		}
        return canMove;
    }
}
