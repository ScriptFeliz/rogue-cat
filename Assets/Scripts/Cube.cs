using UnityEngine;
using System.Collections;

public class Cube {

    public GameObject instance;
    public Cart cartPos;

    // fogWar
    public bool litVisited;

    // unit
    public GameObject unit;

    public Cube(GameObject gameObj, Cart pos)
    {
        instance = gameObj;
        cartPos = pos;
    }
}
