using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NursingGame.Editor.Tools
{
    /// <summary>
    /// A Unity Editor window for generating nursing item ScriptableObjects
    /// </summary>
    public class ItemGenerator : EditorWindow
    {
        [System.Serializable]
        public class NursingItemData
        {
            public string id;
            public string name;
            public Sprite sprite;
            public string description;
        }

        private List<NursingItemData> items = new List<NursingItemData>();
        private Vector2 scrollPosition;
        private string savePath = "Assets/ScriptableObjects/Items";

        [MenuItem("Window/Nursing Game/Item Generator")]
        public static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<ItemGenerator>();
            window.titleContent = new GUIContent("Nursing Item Generator");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Nursing Game Item Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Add New Item"))
            {
                items.Add(new NursingItemData());
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < items.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField($"Item {i + 1}", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);
                
                items[i].id = EditorGUILayout.TextField("ID", items[i].id);
                items[i].name = EditorGUILayout.TextField("Name", items[i].name);
                items[i].sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", items[i].sprite, typeof(Sprite), false);
                items[i].description = EditorGUILayout.TextField("Description", items[i].description);

                EditorGUILayout.Space(5);
                if (GUILayout.Button("Remove Item"))
                {
                    items.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            savePath = EditorGUILayout.TextField("Save Path", savePath);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Generate All Items", GUILayout.Height(30)))
            {
                GenerateItems();
            }
        }

        private void GenerateItems()
        {
            // Create directory if it doesn't exist
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
                if (string.IsNullOrEmpty(itemData.id))
                {
                    Debug.LogWarning("Skipping item with empty ID");
                    continue;
                }

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
            EditorUtility.DisplayDialog("Success", "Nursing items generated successfully!", "OK");
        }
    }
}