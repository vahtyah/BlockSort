using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class LevelDataConverter : EditorWindow
{
    private TextAsset jsonFile;
    private string savePath = "Assets/_Project/Levels";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Level Data Converter")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataConverter>("Level Data Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to LevelData Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // JSON file input
        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);
        
        EditorGUILayout.Space(5);
        
        // Save path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path", GUILayout.Width(60));
        savePath = EditorGUILayout.TextField(savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Save Location", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    savePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Convert button
        GUI.enabled = jsonFile != null;
        if (GUILayout.Button("Convert to LevelData", GUILayout.Height(30)))
        {
            ConvertJsonToLevelData();
        }
        GUI.enabled = true;

        EditorGUILayout.Space(10);

        // Info box
        EditorGUILayout.HelpBox(
            "1. Select a JSON file from your project\n" +
            "2. Choose the save location\n" +
            "3. Click 'Convert to LevelData' to create the ScriptableObject\n\n" +
            "The tool will parse the JSON and create a LevelData asset with all blocks and boxes.",
            MessageType.Info);
    }

    private void ConvertJsonToLevelData()
    {
        try
        {
            // Parse JSON
            JsonLevelData jsonData = JsonUtility.FromJson<JsonLevelData>(jsonFile.text);
            
            if (jsonData == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to parse JSON file!", "OK");
                return;
            }

            // Create LevelData ScriptableObject
            LevelData levelData = CreateInstance<LevelData>();
            levelData.LevelNumber = jsonData.levelNumber;
            levelData.Rows = jsonData.row;
            levelData.Columns = jsonData.column;

            // Convert pieces to blocks
            levelData.Blocks = new List<BlockData>();
            if (jsonData.pieces != null)
            {
                foreach (var piece in jsonData.pieces)
                {
                    BlockData block = new BlockData
                    {
                        Color = GetColorIndex(piece.color),
                        Position = piece.position.ToUnityVector3(),
                        Rotation = piece.rotation.ToUnityQuaternion(),
                        Index = piece.dataIndex,
                        Layer = piece.stackSize
                    };
                    levelData.Blocks.Add(block);
                }
            }

            // Convert boxes
            levelData.Boxes = new List<BoxData>();
            if (jsonData.boxes != null)
            {
                foreach (var box in jsonData.boxes)
                {
                    BoxData boxData = new BoxData
                    {
                        Color = GetColorIndex(box.color),
                        Type = box.boxType,
                        Position = box.position.ToUnityVector3(),
                        Rotation = box.rotation.ToUnityQuaternion()
                    };
                    levelData.Boxes.Add(boxData);
                }
            }

            // Create directory if it doesn't exist
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            // Save as asset
            string assetPath = $"{savePath}/Level_{jsonData.levelNumber}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(levelData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select and ping the created asset
            EditorGUIUtility.PingObject(levelData);
            Selection.activeObject = levelData;

            EditorUtility.DisplayDialog("Success", 
                $"LevelData created successfully!\n\n" +
                $"Level: {jsonData.levelNumber}\n" +
                $"Blocks: {levelData.Blocks.Count}\n" +
                $"Boxes: {levelData.Boxes.Count}\n\n" +
                $"Saved to: {assetPath}", 
                "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Failed to convert JSON:\n\n{ex.Message}\n\n{ex.StackTrace}", 
                "OK");
            Debug.LogError($"Conversion error: {ex}");
        }
    }

    private int GetColorIndex(string colorName)
    {
        // Map color names to indices
        // Adjust these mappings based on your game's color system
        switch (colorName.ToLower())
        {
            case "blue": return 0;
            case "yellow": return 1;
            case "red": return 2;
            case "green": return 3;
            case "purple": return 4;
            case "orange": return 5;
            case "pink": return 6;
            case "cyan": return 7;
            default: return 0;
        }
    }
}

