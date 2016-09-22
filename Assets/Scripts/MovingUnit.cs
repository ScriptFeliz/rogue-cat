using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MovingUnit : Unit {

    private int frameMaxPerMove = 12;

    private Animator animator;
    protected LayerMask blockingLayer;			    //Layer on which collision will be checked.
    
    protected CircleCollider2D circleCollider; 		//The CircleCollider2D component attached to this object.
    protected Rigidbody2D rb2D;				        //The Rigidbody2D component attached to this object.

    protected bool moving;

    public List<Cart> path;
    public Cart nextPos;
    public int skipMoveCount;
    private float _speed;

    public void movingUnitInitialize(Cart startPos, int health, int damage, float speed)
    {
        base.unitInitialize(startPos, health, damage);

        animator = GetComponent<Animator>();
        blockingLayer = LayerMask.NameToLayer("BlockingLayer");
        circleCollider = GetComponent<CircleCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();

        _speed = speed;
        path = new List<Cart>();
    }

    protected virtual void Update()
    {
        if (GameManager.instance.turnLeft <= 0 && !moving)
            animator.SetBool("run", false);
    }

    protected bool computePath(Cart dest)
    {
        path = GameManager.instance.mapManager.findPath(position, dest);
        return path.Count != 0;
    }

    protected IEnumerator smoothMovement(Cart end)
    {
        moving = true;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Vector3 isoDest = end.toIsometric();
        renderer.flipX = isoDest.x > transform.position.x ? false : true;

        //int frameCount = 0;
        float remainingDistance = (isoDest - transform.position).sqrMagnitude;
        while (remainingDistance > Utils.minFloat)
        {
            transform.position = Vector3.MoveTowards(transform.position, isoDest, _speed * Time.deltaTime);
            rb2D.MovePosition (transform.position);
            remainingDistance = (isoDest - transform.position).sqrMagnitude;
            yield return new WaitForEndOfFrame();
            //frameCount++;
            //if (frameCount > frameMaxPerMove)
            //    break;
        }
        position = end;
 
        GameManager.instance.movingUnitIsDone();

        nextPos = null;
        moving = false;
    }

    /*void alternateMove()
    {
        Vector3 end = new Vector3();
        Vector3[] alternateMove =
        {
            new Vector3(),
            new Vector3(),
            new Vector3()
        };
        if (position.x == end.x)
        {
            alternateMove[0] = new Vector3(end.x, end.y - 1f, end.z);
            alternateMove[1] = new Vector3(end.x, end.y + 1f, end.z);
            alternateMove[2] = position.toIsometric();
        }
        else
        {
            alternateMove[0] = new Vector3(end.x - 1f, end.y, end.z);
            alternateMove[1] = new Vector3(end.x + 1f, end.y, end.z);
            alternateMove[2] = position.toIsometric();
        }
        int alternateMoveIndex = 0;
    }*/

    protected bool move(out GameObject unit, out GameObject item)
    {
        unit = null;
		item = null;

        if (skipMoveCount == 0)
        {
            Cart nextMove = null;

            while (path.Count > 0 && path[0] == position)
                path.RemoveAt(0);
            if (path.Count > 0)
                nextMove = path[0];
            if (nextMove != null)
            {
                bool canMove;

                if (GameManager.instance.mapManager.map[nextMove.x][nextMove.y].unit != null)
                {
                    canMove = false;
                    unit = GameManager.instance.mapManager.map[nextMove.x][nextMove.y].unit;
                }
                else
                {
                    canMove = true;
                    nextPos = nextMove;
                    path.RemoveAt(0);
                    GameManager.instance.mapManager.map[position.x][position.y].unit = null;
                    GameManager.instance.mapManager.map[nextMove.x][nextMove.y].unit = gameObject;
                }

				if (GameManager.instance.mapManager.map[nextMove.x][nextMove.y].item != null)
				{
                    item = GameManager.instance.mapManager.map[nextMove.x][nextMove.y].item;
				}

                if (canMove)
                {
                    if (animator.GetBool("run") == false)
                        animator.SetBool("run", true);
                    StartCoroutine(smoothMovement(nextMove));
                    return true;
                }
                //Debug.Log(transform.tag + " cant move pos " + position.toString() + " nextPos " + nextMove.toString());
            }
        }
        else
        {
            skipMoveCount--;
        }

        animator.SetBool("run", false);
        GameManager.instance.movingUnitIsDone();
        return false;
    }

    abstract public bool attemptMove();
}
