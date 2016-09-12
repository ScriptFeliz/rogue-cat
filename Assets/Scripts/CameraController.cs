using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float scrollSpeed = 1f;
	public float pitchSpeed = 0.1f;
	[Range(0.1f, 5f)]
	public float minZoom = 1f;
	[Range(5f, 15f)]
	public float maxZoom = 5f;

	private int lastTouchCount = 0;

	private Vector3 dragOrigin;
	void Update () {
		if (GameManager.instance.cameraFree == true)
		{
			Camera camera = GetComponent<Camera>();

			// mouse move
			if (Input.GetMouseButtonDown(0))
				dragOrigin = camera.ScreenToWorldPoint(Input.mousePosition);
			else if (Input.GetMouseButton(0))
			{
				Vector3 pos = camera.ScreenToWorldPoint(Input.mousePosition);
				Vector3 move = dragOrigin - pos;
				transform.Translate(move);
				dragOrigin = camera.ScreenToWorldPoint(Input.mousePosition);
			}
			// finger move
			if (Input.touchCount == 1 && lastTouchCount == 0)
				dragOrigin = camera.ScreenToWorldPoint(Input.GetTouch(0).position);
			else if (Input.touchCount == 1 && lastTouchCount == 1)
			{
				Vector3 pos = camera.ScreenToWorldPoint(Input.GetTouch(0).position);
				Vector3 move = dragOrigin - pos;
				transform.Translate(move);
				dragOrigin = camera.ScreenToWorldPoint(Input.GetTouch(0).position);
			}

			// scroll zoom
			float scroll = -Input.GetAxis("Mouse ScrollWheel");
			camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + scroll * scrollSpeed, minZoom, maxZoom);
			// pitch zoom
			if (Input.touchCount == 2)
			{
				Touch touchZero = Input.GetTouch(0);
				Touch touchOne = Input.GetTouch(1);

				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + deltaMagnitudeDiff * pitchSpeed, minZoom, maxZoom);
			}

			lastTouchCount = Input.touchCount;
		}
	}
}
