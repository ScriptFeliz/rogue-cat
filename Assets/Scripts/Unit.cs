using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    public float zIndex;
    public int health;
    public int damage;
    public Vector3 position
    {
        get
        {
            return Utils.toCartesian(this.transform.position);
        }
        set
        {
            Vector3 isoPos = Utils.toIsometric(value);
            isoPos.z -= zIndex;
            this.transform.position = isoPos;
        }
    }
}