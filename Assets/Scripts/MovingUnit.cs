using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingUnit : Unit {

    public float speed;
    public bool moving;
    protected List<Vector3> path;

    protected void initMove(Vector3 dest)
    {
        path = GameManager.instance.mapManager.findPath(this.position, dest);

        if (path != null)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetBool("run", true);
        }
    }

    protected IEnumerator move()
    {
        if (path != null)
        {
            Vector3 dest = path[0];
            path.RemoveAt(0);
            if (path.Count == 0)
                path = null;

            SpriteRenderer renderer = this.GetComponent<SpriteRenderer>();
            Vector3 isoDest = Utils.toIsometric(dest);
            renderer.flipX = isoDest.x > transform.position.x ? false : true;

            moving = true;
            float remainingDistance = Vector3.SqrMagnitude(dest - this.position);
            while (remainingDistance > Utils.minFloat)
            {
                this.position = Vector3.MoveTowards(this.position, dest, speed * Time.deltaTime);
                remainingDistance = (dest - this.position).sqrMagnitude;
                yield return null;
            }
            Vector3 pos = this.position;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            this.position = pos;

            moving = false;
        }

        if (path == null)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetBool("run", false);
        }
    }

    virtual public bool attemptMove()
    {
        Debug.Log("attemptMove is not implemented");
        return false;
    }
}