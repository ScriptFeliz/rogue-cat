using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour {

    protected UnitFactoryType unitType;

    public float zIndex;
    public int _health;
    public int _damage;

    private Cart pos;
    public Cart position { get { return pos; } set { pos = value; transform.position = pos.toIsometric(); } }

    protected void unitInitialize (UnitFactoryType type, Cart startPos, int health, int damage)
    {
        unitType = type;

        GameManager.instance.mapManager.map[startPos.x][startPos.y].isTaken = true;
        GameManager.instance.mapManager.map[startPos.x][startPos.y].unitType = unitType;
        GameManager.instance.mapManager.map[startPos.x][startPos.y].unit = gameObject;

        position = startPos;

        _health = health;
        _damage = damage;
    }
}