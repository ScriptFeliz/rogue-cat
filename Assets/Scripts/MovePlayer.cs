using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovePlayer : MonoBehaviour {

    public GameObject PersoMove;
    public float speed = 0.8f;
    private bool moving;
    List<Vector3> path;
	
	// Update is called once per frame
	void Update () {

		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.y);
		if (Input.GetMouseButtonDown(1) && !moving)
          {
              Camera camera = Camera.main;

              Vector3 mouse = Input.mousePosition;
              mouse = camera.ScreenToWorldPoint(mouse);

              mouse = MapManager.ToCartesian(mouse);
              mouse.x = Mathf.Round(mouse.x);
              mouse.y = Mathf.Round(mouse.y);
              Vector3 pos = MapManager.ToIsometric(new Vector3(mouse.x, mouse.y, -1f));
              path = GameManager.instance.mapManager.FindIsoPath(this.transform.position, pos);
              StartCoroutine(MoveTowards(path));
          }
        else if (Input.GetMouseButtonDown(1) && moving)
        {
            for (int i = 1; i < path.Count; ++i)
            {
                path.RemoveAt(i);
            }
        }
    }

	void LateUpdate()
	{
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.y - 0.49f);
	}

    private IEnumerator MoveTowards(List<Vector3> path)
    {
        moving = true;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("run", true);
        while (path.Count > 0)
        {
            float remainingDistance = Vector3.SqrMagnitude(path[0] - this.transform.position);
            if (remainingDistance < float.Epsilon)
            {
			Vector3 position = MapManager.ToCartesian(path[0]);
			position.x = Mathf.Round(position.x);
			position.y = Mathf.Round(position.y);
			this.transform.position = MapManager.ToIsometric(position);
			path.RemoveAt(0);
			continue;
            }
            this.transform.position = Vector3.MoveTowards(this.transform.position, path[0], speed * Time.deltaTime);
            yield return null;
        }
		animator.SetBool("run", false);
		moving = false;
    }

    public void Spawn(Vector3 position)
    {
        this.transform.position = position;
    }
}
