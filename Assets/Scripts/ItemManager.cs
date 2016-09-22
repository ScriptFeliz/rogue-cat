using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ItemManager : MonoBehaviour {

	public ItemList itemList;
	private List<ItemHolder> items = new List<ItemHolder>();

	private class ItemHolder
	{
		public Item item;
		public bool inPlayerInventory;

		public GameObject gameObject;
		public Cart position;

		public ItemHolder(Cart pos, Item item)
		{
			gameObject = new GameObject(item.name);
			SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();

			renderer.sprite = item.sprite;
			position = pos;
			gameObject.transform.position = position.toIsometric();
			this.item = item;
			this.inPlayerInventory = false;
		}
	}

	public GameObject spawnItem(Cart position)
	{
		if (itemList != null)
		{
			items.Add(new ItemHolder(position, itemList.items[Random.Range(0, itemList.items.Count)]));
			return items[items.Count - 1].gameObject;
		}
		return null;
	}

	public void removeNotLootedItems()
	{
		if (itemList != null)
		{
			for (int i = 0; i < items.Count; ++i)
			{
				if (items[i].inPlayerInventory == false)
				{
					Destroy(items[i].gameObject);
					items.RemoveAt(i);
				}
			}
		}
	}

	public void loot(GameObject item)
	{
		foreach (ItemHolder i in items)
		{
			if (i.gameObject == item)
			{
				i.inPlayerInventory = true;
				Destroy(i.gameObject);
				i.gameObject = null;
			}
		}
	}
}
