using UnityEngine;
using System.Collections;
using System;

public class Utils
{

    public static Vector3 tileSizeInUnits = new Vector3(1.0f, 0.75f, 0.5f);
    public static float minFloat = 0.01f;

    public static Vector3 toIsometric(Vector3 localPosition)
    {
        float isoX = (localPosition.x - localPosition.y) * tileSizeInUnits.x / 2f;
        float isoY = (localPosition.x + localPosition.y) * tileSizeInUnits.y / 2f;
        return new Vector3(isoX, isoY, isoY);
    }

    public static Vector3 toCartesian(Vector3 isoPosition)
    {
        isoPosition.x = (float)Math.Round(isoPosition.x, 2);
        isoPosition.y = (float)Math.Round(isoPosition.y, 2);
        float cartx = (isoPosition.x * (2f / tileSizeInUnits.x) + isoPosition.y * (2f / tileSizeInUnits.y));
        cartx = cartx < minFloat && cartx > -1 ? 0f : cartx / 2f;
        float carty = isoPosition.y * (2f / tileSizeInUnits.y) - cartx;
        carty = carty < minFloat && carty > -1 ? 0f : carty;
        return new Vector3(cartx, carty, 0f);
    }

    public static Vector3 roundVector3(Vector3 vec)
    {
        vec.x = Mathf.Round(vec.x);
        vec.y = Mathf.Round(vec.y);
        return vec;
    }
}

