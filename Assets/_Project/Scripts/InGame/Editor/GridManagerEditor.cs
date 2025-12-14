using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private GridManager gridManager;
    
    // Màu sắc cho các buttons (phải khớp với LevelManager)
    private readonly Color[] testColors = new Color[]
    {
        Color.blue,     // 0 - Blue
        Color.yellow,   // 1 - Yellow
        Color.red,      // 2 - Red
        Color.green,    // 3 - Green
        new Color(0.5f, 0f, 0.5f),  // 4 - Purple
        new Color(1f, 0.5f, 0f),    // 5 - Orange
        new Color(1f, 0.75f, 0.8f), // 6 - Pink
        Color.cyan      // 7 - Cyan
    };
    
    private readonly string[] colorNames = new string[]
    {
        "Blue", "Yellow", "Red", "Green", "Purple", "Orange", "Pink", "Cyan"
    };
    
    private void OnEnable()
    {
        gridManager = (GridManager)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        
        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("🎮 GRID MANAGER CONTROLS 🎮", headerStyle);
        EditorGUILayout.Space(5);
        
        // Chỉ hiển thị buttons khi đang play mode
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("⏸ Các buttons sẽ hoạt động khi đang Play Mode", MessageType.Info);
            return;
        }
        
        // Grid Info Section
        DrawGridInfo();
        
        EditorGUILayout.Space(10);
        
        // Debug Buttons Section
        DrawDebugButtons();
        
        EditorGUILayout.Space(10);
        
        // Remove Blocks By Color Section
        DrawRemoveColorButtons();
        
        EditorGUILayout.Space(10);
        
        // Test Buttons Section
        DrawTestButtons();
    }
    
    private void DrawGridInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        EditorGUILayout.LabelField("📊 Grid Information", titleStyle);
        EditorGUILayout.Space(3);
        
        int blockCount = gridManager.GetBlockCount();
        EditorGUILayout.LabelField($"   🧱 Blocks Count: {blockCount}");
        
        // Hiển thị settings hiện tại
        SerializedProperty removeFromBottom = serializedObject.FindProperty("removeFromBottomOnly");
        SerializedProperty maxBlocks = serializedObject.FindProperty("maxBlocksToRemove");
        
        if (removeFromBottom != null && maxBlocks != null)
        {
            string mode = removeFromBottom.boolValue ? "Bottom Row Only ⬇️" : "All Grid 🌐";
            EditorGUILayout.LabelField($"   🎯 Mode: {mode}");
            EditorGUILayout.LabelField($"   🔢 Max Blocks/Click: {maxBlocks.intValue}");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDebugButtons()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        EditorGUILayout.LabelField("🔧 Debug Tools", titleStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        // Print Grid Button
        GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        if (GUILayout.Button("📋 Print Grid", GUILayout.Height(35)))
        {
            gridManager.PrintGrid();
        }
        
        // Count Blocks Button
        GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
        if (GUILayout.Button("🔢 Count Blocks", GUILayout.Height(35)))
        {
            int count = gridManager.GetBlockCount();
            Debug.Log($"[GridManager] 📊 Số blocks còn lại: {count}");
            EditorUtility.DisplayDialog("Block Count", $"Số blocks còn lại: {count}", "OK");
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // Clear Grid Button
        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
        if (GUILayout.Button("🗑️ Clear Grid (Xóa tất cả)", GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("Clear Grid", 
                "Bạn có chắc muốn xóa toàn bộ grid?", 
                "Xóa", "Hủy"))
            {
                gridManager.ClearGrid();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawRemoveColorButtons()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        EditorGUILayout.LabelField("🎨 Remove Blocks By Color (Click để xóa màu)", titleStyle);
        EditorGUILayout.Space(8);
        
        // Row 1: Blue, Yellow, Red, Green
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 4; i++)
        {
            DrawColorButton(i);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(8);
        
        // Row 2: Purple, Orange, Pink, Cyan
        EditorGUILayout.BeginHorizontal();
        for (int i = 4; i < 8; i++)
        {
            DrawColorButton(i);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("💡 Tip: Chỉnh 'Remove From Bottom Only' và 'Max Blocks To Remove' ở trên để thay đổi hành vi xóa", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawColorButton(int colorIndex)
    {
        Color originalBg = GUI.backgroundColor;
        GUI.backgroundColor = testColors[colorIndex];
        
        // Tạo style cho button với màu chữ tương phản
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = GetContrastColor(testColors[colorIndex]);
        buttonStyle.hover.textColor = GetContrastColor(testColors[colorIndex]);
        buttonStyle.active.textColor = GetContrastColor(testColors[colorIndex]);
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fontSize = 11;
        
        if (GUILayout.Button($"⬤ {colorNames[colorIndex]}", buttonStyle, GUILayout.Height(45)))
        {
            Debug.Log($"[GridManagerEditor] 🖱️ Click button {colorNames[colorIndex]}");
            gridManager.RemoveBlocksByColor(colorIndex);
        }
        
        GUI.backgroundColor = originalBg;
    }
    
    private void DrawTestButtons()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        EditorGUILayout.LabelField("🧪 Test Functions", titleStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        // Test All Colors Button
        GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
        if (GUILayout.Button("🎨 Test All Colors\n(Xóa lần lượt)", GUILayout.Height(45)))
        {
            if (EditorUtility.DisplayDialog("Test All Colors", 
                "Xóa lần lượt tất cả màu (mỗi màu cách nhau 2s)?", 
                "Start", "Cancel"))
            {
                gridManager.StartCoroutine(TestAllColorsCoroutine());
            }
        }
        
        // Reload Level Button
        GUI.backgroundColor = new Color(0.2f, 0.8f, 1f);
        if (GUILayout.Button("🔄 Reload Level\n(Tải lại)", GUILayout.Height(45)))
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.RestartLevel();
                Debug.Log("[GridManagerEditor] ✓ Level reloaded!");
            }
            else
            {
                Debug.LogWarning("[GridManagerEditor] ⚠️ LevelManager not found!");
                EditorUtility.DisplayDialog("Error", "Không tìm thấy LevelManager trong scene!", "OK");
            }
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private System.Collections.IEnumerator TestAllColorsCoroutine()
    {
        Debug.Log("[GridManagerEditor] 🎨 Bắt đầu test tất cả màu...");
        
        for (int i = 0; i < colorNames.Length; i++)
        {
            Debug.Log($"[GridManagerEditor] Testing {colorNames[i]}... ({i+1}/{colorNames.Length})");
            gridManager.RemoveBlocksByColor(i);
            yield return new WaitForSeconds(2f);
        }
        
        Debug.Log("[GridManagerEditor] ✅ Hoàn thành test tất cả màu!");
    }
    
    /// <summary>
    /// Lấy màu tương phản cho text trên button
    /// </summary>
    private Color GetContrastColor(Color backgroundColor)
    {
        // Tính độ sáng của màu nền
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
        
        // Nếu nền sáng thì dùng chữ đen, nền tối thì dùng chữ trắng
        return luminance > 0.5f ? Color.black : Color.white;
    }
}

