using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		MapManager controller = target as MapManager;

		for (int i = 0; i < controller.tilemaps.Length; ++i)
		{
			float total = 0f;
			for (int a = 0; a < controller.tilemaps[i].secondaryTiles.Length; ++a)
			{
				if (controller.tilemaps[i].secondaryTiles[a].tilemap == null)
					controller.tilemaps[i].secondaryTiles[a].tilemap = controller.tilemaps[i];
				controller.tilemaps[i].secondaryTiles[a].chance = controller.tilemaps[i].secondaryTiles[a].chance;
				total += controller.tilemaps[i].secondaryTiles[a].chance;
			}
			controller.tilemaps[i].baseTileChance = 100f - total;
		}
	}
}
