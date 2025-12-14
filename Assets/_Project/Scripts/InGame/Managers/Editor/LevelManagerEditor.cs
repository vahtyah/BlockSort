using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    private int levelToLoad = 1;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LevelManager levelManager = (LevelManager)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Level Control", EditorStyles.boldLabel);
        
        // Current level display
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Current Level: {levelManager.GetCurrentLevelNumber()}", EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();
            
            LevelData currentData = levelManager.GetCurrentLevelData();
            if (currentData != null)
            {
                EditorGUILayout.LabelField($"Blocks: {currentData.Blocks.Count} | Boxes: {currentData.Boxes.Count}", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.Space(5);
        }
        
        // Load specific level
        EditorGUILayout.BeginHorizontal();
        levelToLoad = EditorGUILayout.IntField("Level Number", levelToLoad);
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Load Level", GUILayout.Width(100)))
        {
            levelManager.LoadLevel(levelToLoad);
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Quick actions
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("Restart Level"))
        {
            levelManager.RestartLevel();
        }
        
        if (GUILayout.Button("Next Level"))
        {
            levelManager.LoadNextLevel();
        }
        
        if (GUILayout.Button("Clear Level"))
        {
            levelManager.ClearLevel();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Level info
        EditorGUILayout.LabelField("Quick Load", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Application.isPlaying;
        
        for (int i = 1; i <= 5; i++)
        {
            if (GUILayout.Button($"Level {i}"))
            {
                levelManager.LoadLevel(i);
            }
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Info box
        EditorGUILayout.HelpBox(
            "Runtime Controls:\n" +
            "• Load Level - Load level cụ thể\n" +
            "• Restart Level - Load lại level hiện tại\n" +
            "• Next Level - Load level tiếp theo\n" +
            "• Clear Level - Xóa tất cả objects trong level\n\n" +
            "Các button chỉ hoạt động khi game đang chạy (Play Mode)",
            MessageType.Info);
    }
}

