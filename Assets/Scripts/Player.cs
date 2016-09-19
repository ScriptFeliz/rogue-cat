using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MovingUnit {

    public void init ()
    {
        this.transform.position = GameManager.instance.mapManager.spawn.transform.position;
    }

	void Update ()
    {
        Camera camera = Camera.main;

        GameManager.instance.mapManager.litMap(this.position);
	    if (Input.GetMouseButtonDown(0))
        {
            if (path != null)
                path = null;
            else
            {
                Vector3 dest = Utils.toCartesian(camera.ScreenToWorldPoint(Input.mousePosition));
                dest = Utils.roundVector3(dest);
                initMove(dest);
            }
        }
	}

    public override bool attemptMove()
    {
        if (path != null)
        {
            Vector3 nextMove = path[0];
            // Check items an stuffs
            StartCoroutine(move());
        }
        return true;
    }
}
