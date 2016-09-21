using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class AssetHandler
{

    [OnOpenAssetAttribute(1)]
    public static bool handler(int instanceID, int line)
    {
        Object instance = EditorUtility.InstanceIDToObject(instanceID);

        // When double clicking an ItemList in project view,
        // open the ItemListEditor window.
        if (instance is ItemList)
        {
            EditorPrefs.SetInt("ItemListID", instanceID);
            ItemListEditor.Init();
            return true;
        }
        return false;
    }
}

public class ProjectIcons : Editor
{
    [InitializeOnLoadMethod]
    static void EnableIcons()
    {
        EditorApplication.projectWindowItemOnGUI -= ProjectIcons.MyCallback();
        EditorApplication.projectWindowItemOnGUI += ProjectIcons.MyCallback();
    }

    static EditorApplication.ProjectWindowItemCallback MyCallback()
    {
        EditorApplication.ProjectWindowItemCallback myCallback = new EditorApplication.ProjectWindowItemCallback(IconGUI);
        return myCallback;
    }

    // Draw icon over items in project view
    static void IconGUI(string guid, Rect rect)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        Item obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Item)) as Item;
        if (obj != null && obj.sprite != null)
        {
            if (obj.sprite.texture == null)
                return;

            TextureImporter ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj.sprite.texture)) as TextureImporter;
            if (ti == null || !ti.isReadable)
                return;

            rect.height = rect.width;
            Texture2D croppedTexture = new Texture2D((int)obj.sprite.rect.width, (int)obj.sprite.rect.height);
            Color[] pixels = obj.sprite.texture.GetPixels((int)obj.sprite.rect.x,
                                                    (int)obj.sprite.rect.y,
                                                    (int)obj.sprite.rect.width,
                                                    (int)obj.sprite.rect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            GUI.DrawTexture(rect, croppedTexture);
        }
    }
}
