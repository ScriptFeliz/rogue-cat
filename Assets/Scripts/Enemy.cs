using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MovingUnit {

    private Player target;

    public void initialize(Cart startPos, int health, int damage, float speed)
    {
        base.movingUnitInitialize(startPos, health, damage, speed);
    }

    public void setTarget(Player newTarget)
    {
        target = newTarget;
    }
    public float distanceToTarget()
    {
        return position.distanceTo(target.position);
    }

    override public bool attemptMove()
    {
        if (distanceToTarget() < 5f)
        {
            Cart targetPos = (target.nextPos != null) ? target.nextPos : target.position;
            computePath(targetPos);
        }

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
