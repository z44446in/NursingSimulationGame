using System.Collections;

// Assets/Editor/ItemDataGenerator.cs ���� ����
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ItemDataGenerator : EditorWindow
{
    [System.Serializable]
    public class ItemData
    {
        public string id;
        public string name;
        public Sprite sprite;
        public string description;
        public bool isReusable = true;
        public bool requiresConfirmation = true;
    }

    private List<ItemData> items = new List<ItemData>();
    private Vector2 scrollPosition;
    private string savePath = "Assets/ScriptableObjects/Items";

    [MenuItem("Tools/Item Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataGenerator>("Item Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Data Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Item"))
        {
            items.Add(new ItemData());
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < items.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Item {i + 1}");
            items[i].id = EditorGUILayout.TextField("ID", items[i].id);
            items[i].name = EditorGUILayout.TextField("Name", items[i].name);
            items[i].sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", items[i].sprite, typeof(Sprite), false);
            items[i].description = EditorGUILayout.TextField("Description", items[i].description);
            items[i].isReusable = EditorGUILayout.Toggle("Is Reusable", items[i].isReusable);
            items[i].requiresConfirmation = EditorGUILayout.Toggle("Requires Confirmation", items[i].requiresConfirmation);

            if (GUILayout.Button("Remove"))
            {
                items.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Generate All Items"))
        {
            GenerateItems();
        }
    }

    private void GenerateItems()
    {
        // ���� ��ΰ� ������ ����
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string[] folders = savePath.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string parentPath = currentPath;
                currentPath = $"{currentPath}/{folders[i]}";
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }
            }
        }

        foreach (var itemData in items)
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.itemId = itemData.id;
            item.itemName = itemData.name;
            item.itemSprite = itemData.sprite;
            item.description = itemData.description;
            

            string assetPath = $"{savePath}/{itemData.id}.asset";
            AssetDatabase.CreateAsset(item, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Items generated successfully!", "OK");
    }
}