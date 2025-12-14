using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelManagerWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private LevelData[] allLevels;
    private bool showLevelDetails = true;
    private int selectedLevelIndex = -1;
    
    [MenuItem("Tools/Level Manager Window")]
    public static void ShowWindow()
    {
        LevelManagerWindow window = GetWindow<LevelManagerWindow>("Level Manager");
        window.minSize = new Vector2(400, 300);
    }
    
    private void OnEnable()
    {
        RefreshLevelList();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Refresh button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔄 Refresh Level List", GUILayout.Height(25)))
        {
            RefreshLevelList();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        if (allLevels == null || allLevels.Length == 0)
        {
            EditorGUILayout.HelpBox("Không tìm thấy level nào trong Resources/Levels", MessageType.Warning);
            
            if (GUILayout.Button("Open Level Data Converter"))
            {
                LevelDataConverter.ShowWindow();
            }
            
            return;
        }
        
        // Statistics
        DrawStatistics();
        
        EditorGUILayout.Space(10);
        
        // Level list header
        EditorGUILayout.BeginHorizontal();
        showLevelDetails = EditorGUILayout.Foldout(showLevelDetails, $"All Levels ({allLevels.Length})", true);
        EditorGUILayout.EndHorizontal();
        
        if (showLevelDetails)
        {
            DrawLevelList();
        }
        
        EditorGUILayout.Space(10);
        
        // Runtime controls
        DrawRuntimeControls();
    }
    
    private void RefreshLevelList()
    {
        allLevels = LevelLoader.LoadAllLevels();
        
        // Sort by level number
        if (allLevels != null && allLevels.Length > 0)
        {
            allLevels = allLevels.OrderBy(l => l.LevelNumber).ToArray();
        }
    }
    
    private void DrawStatistics()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        
        int totalLevels = allLevels.Length;
        int totalBlocks = 0;
        int totalBoxes = 0;
        
        foreach (var level in allLevels)
        {
            totalBlocks += level.Blocks.Count;
            totalBoxes += level.Boxes.Count;
        }
        
        EditorGUILayout.LabelField($"Total Levels: {totalLevels}");
        EditorGUILayout.LabelField($"Total Blocks: {totalBlocks}");
        EditorGUILayout.LabelField($"Total Boxes: {totalBoxes}");
        
        if (totalLevels > 0)
        {
            EditorGUILayout.LabelField($"Avg Blocks/Level: {totalBlocks / totalLevels}");
            EditorGUILayout.LabelField($"Avg Boxes/Level: {totalBoxes / totalLevels}");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawLevelList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        
        for (int i = 0; i < allLevels.Length; i++)
        {
            LevelData level = allLevels[i];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Level header
            EditorGUILayout.BeginHorizontal();
            
            bool isSelected = selectedLevelIndex == i;
            Color originalColor = GUI.backgroundColor;
            if (isSelected)
            {
                GUI.backgroundColor = Color.yellow;
            }
            
            if (GUILayout.Button($"Level {level.LevelNumber}", EditorStyles.miniButtonLeft, GUILayout.Width(80)))
            {
                selectedLevelIndex = isSelected ? -1 : i;
                EditorGUIUtility.PingObject(level);
            }
            
            GUI.backgroundColor = originalColor;
            
            EditorGUILayout.LabelField($"Grid: {level.Rows}x{level.Columns}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"Blocks: {level.Blocks.Count}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"Boxes: {level.Boxes.Count}", GUILayout.Width(70));
            
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("▶ Load", EditorStyles.miniButtonRight, GUILayout.Width(60)))
            {
                LoadLevelInGame(level.LevelNumber);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // Level details
            if (isSelected)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.ObjectField("Asset", level, typeof(LevelData), false);
                
                if (level.Blocks.Count > 0)
                {
                    EditorGUILayout.LabelField($"Block Indices: {string.Join(", ", level.Blocks.Take(10).Select(b => b.Index))}" +
                        (level.Blocks.Count > 10 ? "..." : ""));
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawRuntimeControls()
    {
        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use runtime controls", MessageType.Info);
            return;
        }
        
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        GameController gameController = FindObjectOfType<GameController>();
        
        if (levelManager == null)
        {
            EditorGUILayout.HelpBox("No LevelManager found in scene", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        int currentLevel = levelManager.GetCurrentLevelNumber();
        EditorGUILayout.LabelField($"Current Level: {currentLevel}", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("⏮ Previous"))
        {
            if (currentLevel > 1)
            {
                levelManager.LoadLevel(currentLevel - 1);
            }
        }
        
        if (GUILayout.Button("🔄 Restart"))
        {
            levelManager.RestartLevel();
        }
        
        if (GUILayout.Button("Next ⏭"))
        {
            levelManager.LoadNextLevel();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void LoadLevelInGame(int levelNumber)
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        
        if (levelManager != null)
        {
            levelManager.LoadLevel(levelNumber);
            Debug.Log($"[LevelManagerWindow] Loaded Level {levelNumber}");
        }
        else
        {
            Debug.LogWarning("[LevelManagerWindow] No LevelManager found in scene");
        }
    }
}

