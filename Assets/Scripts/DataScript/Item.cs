using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName ="Items/Item")]
public class Item : ScriptableObject {

    public bool obtenable;
    public List<Recipe> availableRecipes;
    public Recipe recipe;
    public Sprite sprite;
}
