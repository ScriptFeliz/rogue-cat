using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float speed = 2f;
	public float scrollSpeed = 1f;
	[Range(0.1f, 5f)]
	public float minZoom = 1f;
	[Range(5f, 15f)]
	public float maxZoom = 5f;

	void Update () {
		if (GameManager.instance.cameraFree == true)
		{
			Camera camera = GetComponent<Camera>();
			if (Input.GetMouseButton(0))
			{
				float vertical = -Input.GetAxis("Mouse X") * speed * Time.deltaTime * camera.orthographicSize;
				float horizontal = -Input.GetAxis("Mouse Y") * speed * Time.deltaTime * camera.orthographicSize;
				transform.Translate(vertical, horizontal, 0);
			}
			float scroll = -Input.GetAxis("Mouse ScrollWheel");
			camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + scroll * scrollSpeed, minZoom, maxZoom);
		}
	}
}
