using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MovingUnit {

    private Player target;

    public void initialize(Cart startPos, int health, int damage, float speed)
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        base.movingUnitInitialize(UnitFactoryType.Enemy, startPos, health, damage, speed);
    }

    public bool readyToAttack(out Cart targetPos)
    {
        targetPos = null;
        if (position.distanceTo(target.position) < 5f)
        {
            targetPos = (target.nextPos != null) ? target.nextPos : target.position;
            return true;
        }
        return false;
    }

    override public bool attemptMove()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        Cart targetPos;
        if (readyToAttack(out targetPos))
            computePath(targetPos);

        GameObject unit;
        bool canMove = move(out unit);
        if (!canMove && unit != null)
        {
            if (unit.transform.tag == "Player")
            {
                Player player = target.GetComponent<Player>();
                player._health -= _damage;
                if (player._health <= 0)
                    GameManager.instance.gameOver();
                Debug.Log("Enemy hit player. Your life is now " + player._health);
            }
        }
        return canMove;
    }
}
