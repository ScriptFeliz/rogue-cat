using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName ="Items/ItemList")]
public class ItemList : ScriptableObject {

    public List<Item> items = new List<Item>();
    public List<Recipe> recipes = new List<Recipe>();

    public void removeDirty()
    {
        if (items != null)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i] == null)
                    items.RemoveAt(i--);
            }
        }

        if (recipes != null)
        {
			for (int i = 0; i < recipes.Count; ++i)
			{
				if (recipes[i].result == null)
				{
					recipes.RemoveAt(i--);
					continue;
				}

				for (int a = 0; a < recipes[i].ingredients.Count; ++a)
				{
					if (recipes[i].ingredients[a] == null)
						recipes[i].ingredients.RemoveAt(a);
				}

				if (recipes[i].ingredients.Count == 0)
				{
					recipes[i].result.recipe = null;
					recipes.RemoveAt(i);
				}
			}
        }
    }

	public bool exists(Item item)
	{
		if (items != null)
		{
			foreach (Item i in items)
			{
				if (i == item)
					return true;
			}
		}
		return false;
	}

}
