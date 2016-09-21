using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Recipe {

    public List<Item> ingredients = new List<Item>();
    public Item result;
}
