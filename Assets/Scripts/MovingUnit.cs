using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MovingUnit : Unit {

    private int frameMaxPerMove = 12;

    private Animator animator;
    protected LayerMask blockingLayer;			    //Layer on which collision will be checked.
    
    protected CircleCollider2D circleCollider; 		//The CircleCollider2D component attached to this object.
    protected Rigidbody2D rb2D;				        //The Rigidbody2D component attached to this object.

    private bool moving;

    public List<Cart> path;
    public Cart nextPos;
    public int skipMoveCount;
    private float _speed;

    public void movingUnitInitialize(UnitFactoryType type, Cart startPos, int health, int damage, float speed)
    {
        base.unitInitialize(type, startPos, health, damage);

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

    protected bool getNextMove(out Cart nextMove)
    {
        if (path.Count <= 0)
        {
            nextMove = new Cart();
            return false;
        }
        nextMove = path[0];
        path.RemoveAt(0);
        return true;
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

    protected bool move(out GameObject unit)
    {
        unit = null;

        if (skipMoveCount == 0)
        {
            bool haveNextMove;

            Cart nextMove;
            haveNextMove = getNextMove(out nextMove);
            if (haveNextMove)
            {
                bool canMove;

                if (GameManager.instance.mapManager.map[nextMove.x][nextMove.y].isTaken == true)
                {
                    canMove = false;
                    unit = GameManager.instance.mapManager.map[nextMove.x][nextMove.y].unit;
                }
                else
                {
                    canMove = true;
                    nextPos = nextMove;
                    GameManager.instance.mapManager.map[position.x][position.y].isTaken = false;
                    GameManager.instance.mapManager.map[position.x][position.y].unitType = UnitFactoryType.Undefined;
                    GameManager.instance.mapManager.map[position.x][position.y].unit = null;
                    GameManager.instance.mapManager.map[nextMove.x][nextMove.y].isTaken = true;
                    GameManager.instance.mapManager.map[nextMove.x][nextMove.y].unitType = unitType;
                    GameManager.instance.mapManager.map[nextMove.x][nextMove.y].unit = gameObject;
                }

                if (canMove)
                {
                    if (animator.GetBool("run") == false)
                        animator.SetBool("run", true);
                    StartCoroutine(smoothMovement(nextMove));
                    return true;
                }
                //Debug.Log(transform.tag + " cant move");
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