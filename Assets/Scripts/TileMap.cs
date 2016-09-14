using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[Serializable]
public class TileMap {

	public GameObject BaseTile;
	[SerializeField, ReadOnly]
	private float _baseTileChance; // for editor purpose only

	public secondaryTile[] secondaryTiles;
	public float baseTileChance
	{
		get
		{
			float chance = 0f;
			for (int i = 0; i < secondaryTiles.Length; ++i)
			{
				chance += secondaryTiles[i].chance;
			}
			return Mathf.Clamp(100f - chance, 0f, 100f);
		}
		set
		{
			_baseTileChance = value;
		}
	}


	[Serializable]
	public class secondaryTile
	{
		public GameObject tile;
		[HideInInspector]
		public float chance
		{
			get { return _chance; }
			set
			{
				if (tilemap != null)
				{
					float total = 0f;
					for (int i = 0; i < tilemap.secondaryTiles.Length; ++i)
					{
						if (tilemap.secondaryTiles[i] != this)
							total += tilemap.secondaryTiles[i].chance;
					}
					if (total + value > 100f)
						_chance = 100f - total;
					else
						_chance = value;
				}
				else
					_chance = value;
			}
		}
		[NonSerialized]
		public TileMap tilemap;

		[Range(0, 100), SerializeField]
		private float _chance;
	}

	// take a value clamped to the range [0-100] and returns
	// the corresponding tile based on their chance to appear 
	public GameObject GetTile(float value)
	{
		float chance = 0f;
		for (int i = 0; i < secondaryTiles.Length; ++i)
		{
			chance += secondaryTiles[i].chance;
			if (value <= chance)
				return secondaryTiles[i].tile;
		}
		return BaseTile;
	}
}

public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property,
											GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position,
							   SerializedProperty property,
							   GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}
