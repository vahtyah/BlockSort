using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GameController gameController = (GameController)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Game Control", EditorStyles.boldLabel);
        
        // Current state
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Current Level: {gameController.GetCurrentLevel()}");
            EditorGUILayout.LabelField($"Highest Level: {gameController.GetHighestLevel()}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }
        
        // Game control buttons
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("▶ Start Game"))
        {
            gameController.StartGame();
        }
        
        if (GUILayout.Button("⏸ Pause"))
        {
            gameController.PauseGame();
        }
        
        if (GUILayout.Button("▶ Resume"))
        {
            gameController.ResumeGame();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Level control
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("🔄 Restart"))
        {
            gameController.RestartLevel();
        }
        
        if (GUILayout.Button("➡ Next Level"))
        {
            gameController.NextLevel();
        }
        
        if (GUILayout.Button("✓ Complete"))
        {
            gameController.CompleteLevel();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Progress control
        EditorGUILayout.LabelField("Progress Control", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("Reset Progress (Back to Level 1)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset Progress", 
                "Bạn có chắc muốn reset tiến trình về Level 1?", 
                "Yes", "No"))
            {
                gameController.ResetProgress();
            }
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // Keyboard shortcuts info
        EditorGUILayout.HelpBox(
            "Keyboard Shortcuts (In Play Mode):\n" +
            "• N - Next Level\n" +
            "• R - Restart Level\n" +
            "• 1/2/3 - Load Level 1/2/3\n\n" +
            "Buttons chỉ hoạt động khi game đang chạy (Play Mode)",
            MessageType.Info);
    }
}

