using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class BatchLevelDataConverter : EditorWindow
{
    private string sourceFolder = "Assets/RemovableFile/TextAsset";
    private string targetFolder = "Assets/_Project/Levels";
    private List<TextAsset> jsonFiles = new List<TextAsset>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Batch Level Data Converter")]
    public static void ShowWindow()
    {
        GetWindow<BatchLevelDataConverter>("Batch Level Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch JSON to LevelData Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // Source folder
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Source Folder", GUILayout.Width(100));
        sourceFolder = EditorGUILayout.TextField(sourceFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Source Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    sourceFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // Target folder
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target Folder", GUILayout.Width(100));
        targetFolder = EditorGUILayout.TextField(targetFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Target Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    targetFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Scan button
        if (GUILayout.Button("Scan for JSON Files", GUILayout.Height(25)))
        {
            ScanForJsonFiles();
        }

        EditorGUILayout.Space(10);

        // Display found files
        if (jsonFiles.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {jsonFiles.Count} JSON file(s):", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (var file in jsonFiles)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(file.name, GUILayout.Width(150));
                EditorGUILayout.ObjectField(file, typeof(TextAsset), false);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Convert all button
            if (GUILayout.Button("Convert All to LevelData", GUILayout.Height(30)))
            {
                ConvertAllJsonFiles();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No JSON files found. Click 'Scan for JSON Files' to search.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // Info box
        EditorGUILayout.HelpBox(
            "Batch Converter:\n" +
            "1. Set the source folder containing JSON files\n" +
            "2. Set the target folder for LevelData assets\n" +
            "3. Click 'Scan for JSON Files' to find all JSON files\n" +
            "4. Click 'Convert All to LevelData' to process all files\n\n" +
            "This will automatically convert all JSON level files to LevelData assets.",
            MessageType.Info);
    }

    private void ScanForJsonFiles()
    {
        jsonFiles.Clear();

        if (!Directory.Exists(sourceFolder))
        {
            EditorUtility.DisplayDialog("Error", $"Source folder does not exist:\n{sourceFolder}", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { sourceFolder });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            
            if (textAsset != null && IsValidJson(textAsset.text))
            {
                jsonFiles.Add(textAsset);
            }
        }

        jsonFiles = jsonFiles.OrderBy(f => f.name).ToList();
        Debug.Log($"Found {jsonFiles.Count} JSON file(s) in {sourceFolder}");
    }

    private bool IsValidJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        text = text.Trim();
        return (text.StartsWith("{") && text.EndsWith("}")) || 
               (text.StartsWith("[") && text.EndsWith("]"));
    }

    private void ConvertAllJsonFiles()
    {
        if (jsonFiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No JSON files to convert!", "OK");
            return;
        }

        // Create target directory if it doesn't exist
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        int successCount = 0;
        int failCount = 0;
        List<string> errors = new List<string>();

        for (int i = 0; i < jsonFiles.Count; i++)
        {
            TextAsset jsonFile = jsonFiles[i];
            
            if (EditorUtility.DisplayCancelableProgressBar(
                "Converting JSON Files", 
                $"Processing {jsonFile.name} ({i + 1}/{jsonFiles.Count})", 
                (float)i / jsonFiles.Count))
            {
                break;
            }

            try
            {
                if (ConvertSingleFile(jsonFile))
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    errors.Add($"Failed to parse: {jsonFile.name}");
                }
            }
            catch (System.Exception ex)
            {
                failCount++;
                errors.Add($"{jsonFile.name}: {ex.Message}");
                Debug.LogError($"Error converting {jsonFile.name}: {ex}");
            }
        }

        EditorUtility.ClearProgressBar();

        // Show summary
        string message = $"Conversion Complete!\n\n" +
                        $"Successful: {successCount}\n" +
                        $"Failed: {failCount}\n\n" +
                        $"Assets saved to: {targetFolder}";

        if (errors.Count > 0)
        {
            message += "\n\nErrors:\n" + string.Join("\n", errors);
        }

        EditorUtility.DisplayDialog("Batch Conversion Complete", message, "OK");
        AssetDatabase.Refresh();
    }

    private bool ConvertSingleFile(TextAsset jsonFile)
    {
        // Parse JSON
        JsonLevelData jsonData = JsonUtility.FromJson<JsonLevelData>(jsonFile.text);
        
        if (jsonData == null) return false;

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

        // Save as asset
        string assetPath = $"{targetFolder}/Level_{jsonData.levelNumber}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(levelData, assetPath);
        
        Debug.Log($"Created: {assetPath} (Blocks: {levelData.Blocks.Count}, Boxes: {levelData.Boxes.Count})");
        
        return true;
    }

    private int GetColorIndex(string colorName)
    {
        // Map color names to indices
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

