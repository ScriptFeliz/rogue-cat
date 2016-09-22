using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ItemListEditor : EditorWindow
{
	private ItemList itemList;
	private string itemListPath;
	private int instanceID;
	private int viewIndex;
	private int newViewIndex; // because changing viewIndex during drag'n'drop causes a crash.
	private static Texture2D winLogo;

	// for item selector drop down menu
	private string[] itemNames
	{
		get
		{
			string[] itemNames = new string[itemList.items.Count];
			for (int i = 0; i < itemList.items.Count; ++i)
			{
				itemNames[i] = itemList.items[i].name == "" ? "item " + i : itemList.items[i].name;
			}
			return itemNames;
		}
		set { return; }
	}

	[MenuItem("Custom/ItemList Editor")]
	public static void Init()
	{
		ItemListEditor window = (ItemListEditor)EditorWindow.GetWindow(typeof(ItemListEditor));
		window.Config();
		window.Show();
	}

	private void OnGUI()
	{
		if (!itemList)
			return;
		itemList.removeDirty();

		/* Title bar */
		EditorGUILayout.BeginHorizontal(
			bgColor(new Color(1f, 1f, 1f, 0.5f),
				padding(5, 5, 5, 5, new GUIStyle())));

		GUILayout.Label(winLogo, GUILayout.ExpandWidth(false));
		EditorGUILayout.LabelField(itemList.name, EditorStyles.boldLabel);

		EditorGUILayout.EndHorizontal();
		/* Title bar */

		GUILayout.Space(2);

		/* Drag'n'drop box */
		Event evt = Event.current;
		Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
		GUIStyle style = GUI.skin.box;
		style.alignment = TextAnchor.MiddleCenter;
		GUI.Box(drop_area, "Drag item here", style);

		switch (evt.type)
		{
			case EventType.DragUpdated:
				if (!drop_area.Contains(evt.mousePosition))
					break;
				foreach (Object dragged_object in DragAndDrop.objectReferences)
				{
					if (!(dragged_object is Item))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						break;
					}
				}
				if (DragAndDrop.visualMode != DragAndDropVisualMode.Rejected)
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
				break;

			case EventType.DragPerform:
				if (!drop_area.Contains(evt.mousePosition))
					break;
				DragAndDrop.AcceptDrag();
				foreach (Object dragged_object in DragAndDrop.objectReferences)
				{
					bool exists = false;
					Item item = dragged_object as Item;
					for (int i = 0; i < itemList.items.Count; ++i)
					{
						if (itemList.items[i] == item)
						{
							viewIndex = i;
							exists = true;
						}
					}
					if (!exists)
					{
						itemList.items.Add(item);
						viewIndex = itemList.items.Count - 1;
					}
				}
				return;
		}
		/* Drag'n'drop box */

		/* Item selector */
		EditorGUILayout.BeginHorizontal();

		EditorStyles.popup.fixedHeight = 15;
		viewIndex = EditorGUILayout.Popup(viewIndex, itemNames, GUILayout.Width(this.minSize.x / 3f));

		// Add item to the list or open one.
		if (GUILayout.Button("Add / Open", GUILayout.ExpandWidth(false), GUILayout.Height(15))) // Add item
		{
			string absPath = EditorUtility.OpenFilePanel("Select Inventory Item List", "Assets/Data", "asset");
			if (absPath.StartsWith(Application.dataPath))
			{
				string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
				Item item = AssetDatabase.LoadAssetAtPath<Item>(relPath);
				if (item)
				{
					bool exists = false;
					for (int i = 0; i < itemList.items.Count; ++i)
					{
						if (itemList.items[i] == item)
						{
							exists = true;
							viewIndex = i;
							break;
						}
					}
					if (!exists)
					{
						itemList.items.Add(item);
						viewIndex = itemList.items.Count - 1;
					}
				}
			}
		}

		// Delete item from the list
		if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false), GUILayout.Height(15)))
		{
			if (itemList.items != null && itemList.items.Count > 0)
			{
				itemList.items.RemoveAt(viewIndex);
				if (itemList.items.Count == viewIndex)
					--viewIndex;
			}
		}

		// Save ItemList
		if (GUILayout.Button("Save", GUILayout.ExpandWidth(false), GUILayout.Height(15)))
		{
			if (itemList.items != null && itemList.items.Count > 0)
			{
				EditorUtility.SetDirty(itemList);
				foreach (Item item in itemList.items)
					EditorUtility.SetDirty(item);
			}
		}	

		EditorGUILayout.EndHorizontal();
		/* Item selector */

		EditorGUILayout.BeginVertical();
		if (viewIndex >= 0 && viewIndex < itemList.items.Count)
		{
			Item item = itemList.items[viewIndex];

			EditorGUILayout.BeginVertical(GUILayout.Width(this.minSize.x / 2f));
			item.obtenable = EditorGUILayout.Toggle("Can spawn on map", item.obtenable);
			item.sprite = EditorGUILayout.ObjectField("Sprite ", item.sprite, typeof(Sprite), false) as Sprite;
			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			EditorGUILayout.LabelField("recipe");
			EditorGUILayout.BeginHorizontal();
			if (item.recipe != null)
			{
				int width = 0;
				for (int i = 0; i < item.recipe.ingredients.Count; ++i)
				{
					GUILayout.Space(10);
					++width;
					if (recipeDragNDrop(item.recipe.ingredients[i], i))
						return;

					if (GUILayout.Button("X", GUILayout.ExpandWidth(false), GUILayout.Height(15)))
					{
						item.recipe.ingredients.RemoveAt(i);
						return;
					}

					if ((int)(this.position.width / 75f) < width + 2)
					{
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(10);
						EditorGUILayout.BeginHorizontal();
						width = 0;
					}
				}
			}
			GUILayout.Space(10);
			if (recipeDragNDrop(null, 0))
				return;
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		AssetDatabase.SaveAssets();
	}

	private void OnFocus()
	{
		Config();
		Load();
	}

	// repaint and reload asset 10 times a second
	private void OnInspectorUpdate()
	{
		this.Repaint();
	}

	// when selecting an asset in the project view, load it.
	// when selecting multiple assets, load the last.
	void OnSelectionChange()
	{
		if (Selection.instanceIDs.Length < 1)
			return;

		Object instance = EditorUtility.InstanceIDToObject(Selection.instanceIDs[Selection.instanceIDs.Length - 1]);
		if (instance is ItemList)
		{
			EditorPrefs.SetInt("ItemListID", Selection.instanceIDs[Selection.instanceIDs.Length - 1]);
			EditorWindow.GetWindow<ItemListEditor>();
			Load();
		}
		else if (instance is Item && itemList != null)
		{
			for (int i = 0; i < itemList.items.Count; ++i)
			{
				if (itemList.items[i] == instance)
				{
					viewIndex = i;
					break;
				}
			}
		}
	}

	private void Load()
	{
		if (EditorPrefs.HasKey("ItemListID"))
		{
			instanceID = EditorPrefs.GetInt("ItemListID");
			itemListPath = AssetDatabase.GetAssetPath(instanceID);
			itemList = AssetDatabase.LoadAssetAtPath<ItemList>(itemListPath);

			EditorPrefs.DeleteKey("ItemListID");
		}
	}

	private void Config()
	{
		this.minSize = new Vector2(280, 50);
		winLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Images/itemIcon.png");
	}

	private GUIStyle bgColor(Color col, GUIStyle style)
	{
		Color[] pix = new Color[1 * 1];

		for (int i = 0; i < pix.Length; i++)
			pix[i] = col;

		Texture2D result = new Texture2D(1, 1);
		result.SetPixels(pix);
		result.Apply();

		style.normal.background = result;
		return style;
	}

	private GUIStyle padding(int left, int right, int top, int bot, GUIStyle style)
	{
		style.padding = new RectOffset(left, right, top, bot);
		return style;
	}

	private GUIStyle paddingLeft(int left, GUIStyle style)
	{
		style.padding.left = left;
		return style;
	}

	private GUIStyle paddingRight(int right, GUIStyle style)
	{
		style.padding.right = right;
		return style;
	}

	private GUIStyle paddingTop(int top, GUIStyle style)
	{
		style.padding.top = top;
		return style;
	}

	private bool recipeDragNDrop(Item item, int index)
	{
		Event evt = Event.current;
		Rect drop_area = GUILayoutUtility.GetRect(75.0f, 75.0f, GUILayout.ExpandWidth(false));
		GUIStyle style = GUI.skin.box;
		style.alignment = TextAnchor.MiddleCenter;

		if (item != null)
		{
			Texture2D texture = getItemtexture(item);
			if (texture == null)
				GUI.Box(drop_area, item.name, style);
			else
				GUI.Box(drop_area, texture, style);
		}
		else
			GUI.Box(drop_area, "empty", style);

		switch (evt.type)
		{
			case EventType.DragUpdated:
				if (!drop_area.Contains(evt.mousePosition))
					return false;
				if (DragAndDrop.objectReferences.Length > 1 && item != null)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					return false;
				}

				foreach (Object dragged_object in DragAndDrop.objectReferences)
				{
					if (!(dragged_object is Item))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						return false;
					}

					Item dragged_item = dragged_object as Item;
					if (dragged_item == item || dragged_item == itemList.items[viewIndex] || !itemList.exists(dragged_item))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						return false;
					}
				}
				if (DragAndDrop.visualMode != DragAndDropVisualMode.Rejected)
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
				break;

			case EventType.DragPerform:
				if (!drop_area.Contains(evt.mousePosition))
					return false;
				DragAndDrop.AcceptDrag();
				if (item == null)
				{
					foreach (Object dragged_object in DragAndDrop.objectReferences)
					{
						if (itemList.recipes == null)
							itemList.recipes = new List<Recipe>();
						if (itemList.items[viewIndex].recipe == null)
						{
							itemList.items[viewIndex].recipe = new Recipe();
							itemList.items[viewIndex].recipe.result = itemList.items[viewIndex];
							itemList.recipes.Add(itemList.items[viewIndex].recipe);
						}
						itemList.items[viewIndex].recipe.ingredients.Add(dragged_object as Item);
					}
				}
				else
				{
					Item dragged_item = DragAndDrop.objectReferences[0] as Item;
					itemList.items[viewIndex].recipe.ingredients[index] = dragged_item;
				}
				return true;
		}
		return false;
	}

	private class ItemTexture
	{
		public Item item;
		public Texture2D texture;
		public Sprite sprite;
	}

	private static List<ItemTexture> itemTextureList = new List<ItemTexture>();
	private static Texture2D getItemtexture(Item item)
	{
		for (int i = 0; i < itemTextureList.Count; ++i)
		{
			if (itemTextureList[i].item == item)
			{
				if (itemTextureList[i].sprite != item.sprite)
				{
					itemTextureList.RemoveAt(i);
					break;
				}
				return itemTextureList[i].texture;
			}
		}

		ItemTexture itemTexture = new ItemTexture();
		itemTexture.item = item;
		TextureImporter ti = null;

		if (item.sprite != null)
			ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(item.sprite.texture)) as TextureImporter;
		if (ti == null || !ti.isReadable)
		{
			itemTexture.texture = null;
			itemTexture.sprite = null;
		}
		else
		{
			Texture2D croppedTexture = new Texture2D((int)item.sprite.rect.width, (int)item.sprite.rect.height);
			Color[] pixels = item.sprite.texture.GetPixels((int)item.sprite.rect.x,
													(int)item.sprite.rect.y,
													(int)item.sprite.rect.width,
													(int)item.sprite.rect.height);
			croppedTexture.SetPixels(pixels);
			croppedTexture.Apply();
			itemTexture.texture = croppedTexture;
			itemTexture.sprite = item.sprite;
		}

		itemTextureList.Add(itemTexture);
		return itemTexture.texture;
	}
}
