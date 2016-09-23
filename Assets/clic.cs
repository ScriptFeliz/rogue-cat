using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown("Mouse1"))
            GetComponent<Animation>().Play("mallowSeduction");

    }
}