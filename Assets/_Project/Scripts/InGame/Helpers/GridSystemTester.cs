using UnityEngine;

/// <summary>
/// Script để test Grid System
/// Attach vào GameObject và sử dụng các phím tắt để test
/// </summary>
public class GridSystemTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LevelManager levelManager;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableKeyboardShortcuts = true;
    
    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
            
        LogInstructions();
    }
    
    private void Update()
    {
        if (!enableKeyboardShortcuts) return;
        
        // Phím 1-8: Xóa blocks theo màu
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TestRemoveColor(0, "Blue");
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            TestRemoveColor(1, "Yellow");
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            TestRemoveColor(2, "Red");
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            TestRemoveColor(3, "Green");
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            TestRemoveColor(4, "Purple");
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            TestRemoveColor(5, "Orange");
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            TestRemoveColor(6, "Pink");
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            TestRemoveColor(7, "Cyan");
        
        // Phím P: Print Grid
        if (Input.GetKeyDown(KeyCode.P))
            PrintGrid();
        
        // Phím C: Count Blocks
        if (Input.GetKeyDown(KeyCode.C))
            CountBlocks();
        
        // Phím R: Restart Level
        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();
        
        // Phím N: Next Level
        if (Input.GetKeyDown(KeyCode.N))
            NextLevel();
    }
    
    private void TestRemoveColor(int colorIndex, string colorName)
    {
        if (gridManager == null)
        {
            Debug.LogError("[Tester] GridManager not found!");
            return;
        }
        
        // Get color from LevelManager's color array
        Color targetColor = GetColorByIndex(colorIndex);
        
        Debug.Log($"[Tester] Testing remove {colorName} blocks...");
        gridManager.RemoveBlocksByColor(colorIndex);
    }
    
    private Color GetColorByIndex(int index)
    {
        // Phải khớp với mảng màu trong LevelManager
        Color[] colors = new Color[]
        {
            Color.blue,     // 0
            Color.yellow,   // 1
            Color.red,      // 2
            Color.green,    // 3
            new Color(0.5f, 0f, 0.5f),  // 4 - Purple
            new Color(1f, 0.5f, 0f),    // 5 - Orange
            new Color(1f, 0.75f, 0.8f), // 6 - Pink
            Color.cyan      // 7
        };
        
        if (index >= 0 && index < colors.Length)
            return colors[index];
        
        return Color.white;
    }
    
    [ContextMenu("Print Grid")]
    private void PrintGrid()
    {
        if (gridManager == null)
        {
            Debug.LogError("[Tester] GridManager not found!");
            return;
        }
        
        gridManager.PrintGrid();
    }
    
    [ContextMenu("Count Blocks")]
    private void CountBlocks()
    {
        if (gridManager == null)
        {
            Debug.LogError("[Tester] GridManager not found!");
            return;
        }
        
        int count = gridManager.GetBlockCount();
        Debug.Log($"[Tester] Số blocks còn lại: {count}");
    }
    
    [ContextMenu("Restart Level")]
    private void RestartLevel()
    {
        if (levelManager == null)
        {
            Debug.LogError("[Tester] LevelManager not found!");
            return;
        }
        
        Debug.Log("[Tester] Restarting level...");
        levelManager.RestartLevel();
    }
    
    [ContextMenu("Next Level")]
    private void NextLevel()
    {
        if (levelManager == null)
        {
            Debug.LogError("[Tester] LevelManager not found!");
            return;
        }
        
        Debug.Log("[Tester] Loading next level...");
        levelManager.LoadNextLevel();
    }
    
    [ContextMenu("Test All Colors")]
    private void TestAllColors()
    {
        Debug.Log("[Tester] Testing all colors in sequence...");
        StartCoroutine(TestAllColorsCoroutine());
    }
    
    private System.Collections.IEnumerator TestAllColorsCoroutine()
    {
        string[] colorNames = { "Blue", "Yellow", "Red", "Green", "Purple", "Orange", "Pink", "Cyan" };
        
        for (int i = 0; i < colorNames.Length; i++)
        {
            Debug.Log($"[Tester] Testing {colorNames[i]}...");
            TestRemoveColor(i, colorNames[i]);
            yield return new WaitForSeconds(2f);
        }
        
        Debug.Log("[Tester] All colors tested!");
    }
    
    private void LogInstructions()
    {
        if (!enableKeyboardShortcuts) return;
        
        Debug.Log("========================================");
        Debug.Log("GRID SYSTEM TESTER - KEYBOARD SHORTCUTS");
        Debug.Log("========================================");
        Debug.Log("1-8: Xóa blocks theo màu (1=Blue, 2=Yellow, ...)");
        Debug.Log("P: Print Grid");
        Debug.Log("C: Count Blocks");
        Debug.Log("R: Restart Level");
        Debug.Log("N: Next Level");
        Debug.Log("========================================");
        Debug.Log("Hoặc sử dụng Context Menu (Right-click component)");
        Debug.Log("========================================");
    }
}

