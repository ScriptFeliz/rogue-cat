using UnityEngine;
using System.Collections;

public class Cart {

    public int x;
    public int y;

    public int sqrMagnitude { get { return x * x + y * y; } }
    public float magnitude { get { return (float)Mathf.Sqrt(x * x + y * y); } }

    public Cart()
    {
        x = 0;
        y = 0;
    }

    public Cart(int xx, int yy)
    {
        x = xx;
        y = yy;
    }

    public Cart(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        x = (int)pos.x;
        y = (int)pos.y;
    }

    public static Cart operator +(Cart a, Cart b) {  return new Cart(a.x + b.x, a.y + b.y); }
    public static Cart operator -(Cart a, Cart b) {  return new Cart(a.x - b.x, a.y - b.y); }
    public static Cart operator *(Cart a, Cart b) {  return new Cart(a.x * b.x, a.y * b.y); }
    public static Cart operator /(Cart a, Cart b) {  return new Cart(a.x / b.x, a.y / b.y); }

    public Vector3 toIsometric()
    {
        float isoX = (x - y) * Utils.tileSizeInUnits.x / 2f;
        float isoY = (x + y) * Utils.tileSizeInUnits.y / 2f;
        return new Vector3(isoX, isoY, isoY);
    }

    public float distanceTo(Cart a)
    {
        return Mathf.Sqrt((a.x - x) * (a.x - x) + (a.y - y) * (a.y - y));
    }

    public Vector3 toVector3()
    {
        return new Vector3(x, y);
    }
}
