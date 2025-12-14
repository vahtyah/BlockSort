using UnityEngine;

/// <summary>
/// Example script để tích hợp Level System vào game
/// Đặt script này vào một GameObject trong scene
/// </summary>
public class LevelSystemExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameController gameController;
    
    private void Start()
    {
        // Auto-find nếu chưa gán
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
        
        if (gameController == null)
            gameController = FindObjectOfType<GameController>();
    }
    
    // ========== EXAMPLES ==========
    
    /// <summary>
    /// Example: Load level khi player click button
    /// Gọi từ UI Button onClick event
    /// </summary>
    public void OnLevelSelectButtonClick(int levelNumber)
    {
        if (gameController != null)
        {
            gameController.LoadSpecificLevel(levelNumber);
        }
    }
    
    /// <summary>
    /// Example: Next level khi player hoàn thành
    /// Gọi khi detect win condition
    /// </summary>
    public void OnPlayerWin()
    {
        Debug.Log("Player won! Loading next level...");
        
        if (gameController != null)
        {
            gameController.CompleteLevel();
        }
    }
    
    /// <summary>
    /// Example: Restart level khi player thua hoặc click retry
    /// </summary>
    public void OnPlayerRetry()
    {
        Debug.Log("Restarting level...");
        
        if (gameController != null)
        {
            gameController.RestartLevel();
        }
    }
    
    /// <summary>
    /// Example: Pause/Resume game
    /// </summary>
    public void OnPauseButtonClick()
    {
        if (gameController != null)
        {
            if (Time.timeScale == 1f)
            {
                gameController.PauseGame();
                Debug.Log("Game Paused");
            }
            else
            {
                gameController.ResumeGame();
                Debug.Log("Game Resumed");
            }
        }
    }
    
    /// <summary>
    /// Example: Lấy thông tin level hiện tại
    /// </summary>
    public void ShowCurrentLevelInfo()
    {
        if (levelManager != null)
        {
            LevelData levelData = levelManager.GetCurrentLevelData();
            int levelNumber = levelManager.GetCurrentLevelNumber();
            
            if (levelData != null)
            {
                Debug.Log($"=== LEVEL {levelNumber} INFO ===");
                Debug.Log($"Grid Size: {levelData.Rows}x{levelData.Columns}");
                Debug.Log($"Total Blocks: {levelData.Blocks.Count}");
                Debug.Log($"Total Boxes: {levelData.Boxes.Count}");
            }
        }
    }
    
    /// <summary>
    /// Example: Tạo level selection menu
    /// </summary>
    public void CreateLevelSelectionMenu()
    {
        // Load tất cả levels
        LevelData[] allLevels = LevelLoader.LoadAllLevels();
        
        // Lấy level cao nhất đã unlock
        int highestLevel = gameController != null ? 
            gameController.GetHighestLevel() : 1;
        
        Debug.Log($"Total Levels: {allLevels.Length}");
        Debug.Log($"Unlocked up to: Level {highestLevel}");
        
        // Có thể dùng để tạo UI buttons
        foreach (var level in allLevels)
        {
            bool isLocked = level.LevelNumber > highestLevel;
            Debug.Log($"Level {level.LevelNumber}: {(isLocked ? "🔒 Locked" : "✓ Unlocked")}");
        }
    }
    
    /// <summary>
    /// Example: Reset game progress về Level 1
    /// </summary>
    public void OnResetProgressButtonClick()
    {
        if (gameController != null)
        {
            gameController.ResetProgress();
            Debug.Log("Progress reset to Level 1");
        }
    }
    
    /// <summary>
    /// Example: Check nếu level tồn tại trước khi load
    /// </summary>
    public void SafeLoadLevel(int levelNumber)
    {
        if (LevelLoader.LevelExists(levelNumber))
        {
            if (gameController != null)
            {
                gameController.LoadSpecificLevel(levelNumber);
                Debug.Log($"Loading Level {levelNumber}");
            }
        }
        else
        {
            Debug.LogWarning($"Level {levelNumber} không tồn tại!");
        }
    }
    
    // ========== DEBUG FUNCTIONS ==========
    
    /// <summary>
    /// Debug: Test load tất cả levels tuần tự
    /// </summary>
    [ContextMenu("Debug: Test All Levels")]
    public void DebugTestAllLevels()
    {
        LevelData[] allLevels = LevelLoader.LoadAllLevels();
        Debug.Log($"Testing {allLevels.Length} levels...");
        
        foreach (var level in allLevels)
        {
            Debug.Log($"✓ Level {level.LevelNumber} - " +
                     $"Blocks: {level.Blocks.Count}, " +
                     $"Boxes: {level.Boxes.Count}");
        }
    }
    
    /// <summary>
    /// Debug: Show current game state
    /// </summary>
    [ContextMenu("Debug: Show Game State")]
    public void DebugShowGameState()
    {
        if (gameController != null)
        {
            Debug.Log("=== GAME STATE ===");
            Debug.Log($"Current Level: {gameController.GetCurrentLevel()}");
            Debug.Log($"Highest Level: {gameController.GetHighestLevel()}");
            Debug.Log($"Time Scale: {Time.timeScale}");
        }
        
        if (levelManager != null)
        {
            LevelData data = levelManager.GetCurrentLevelData();
            if (data != null)
            {
                Debug.Log($"Loaded Level: {data.LevelNumber}");
                Debug.Log($"Grid: {data.Rows}x{data.Columns}");
            }
        }
    }
}

