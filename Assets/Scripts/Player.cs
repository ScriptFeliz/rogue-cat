using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MovingUnit {

    public void initialize(Cart startPos, int health, int damage, float speed)
    {
        base.movingUnitInitialize(startPos, health, damage, speed);
    }

	override protected void Update ()
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

    override public bool attemptMove()
    {
        GameObject unit;
        bool canMove = move(out unit);
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
        return canMove;
    }
}
