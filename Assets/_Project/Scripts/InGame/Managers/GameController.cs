using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    
    [Header("Game Settings")]
    [SerializeField] private bool autoLoadNextLevel = false;
    [SerializeField] private float nextLevelDelay = 2f;
    
    private int currentLevel = 1;
    private bool isGameActive = false;
    
    private void Awake()
    {
        // Auto-find LevelManager nếu chưa gán
        if (levelManager == null)
        {
            levelManager = FindObjectOfType<LevelManager>();
            
            if (levelManager == null)
            {
                Debug.LogError("[GameController] Không tìm thấy LevelManager trong scene!");
            }
        }
    }
    
    private void Start()
    {
        StartGame();
    }
    
    private void Update()
    {
        // Cheat keys cho testing
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextLevel();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadSpecificLevel(1);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadSpecificLevel(2);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadSpecificLevel(3);
        }
    }
    
    /// <summary>
    /// Bắt đầu game
    /// </summary>
    public void StartGame()
    {
        isGameActive = true;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        
        if (levelManager != null)
        {
            levelManager.LoadLevel(currentLevel);
        }
        
        Debug.Log($"[GameController] Game started - Level {currentLevel}");
    }
    
    /// <summary>
    /// Kết thúc level (player thắng)
    /// </summary>
    public void CompleteLevel()
    {
        if (!isGameActive) return;
        
        Debug.Log($"[GameController] Level {currentLevel} completed!");
        
        // Lưu tiến trình
        SaveProgress();
        
        // Load level tiếp theo
        if (autoLoadNextLevel)
        {
            Invoke(nameof(NextLevel), nextLevelDelay);
        }
    }
    
    /// <summary>
    /// Thất bại level
    /// </summary>
    public void FailLevel()
    {
        if (!isGameActive) return;
        
        Debug.Log($"[GameController] Level {currentLevel} failed!");
    }
    
    /// <summary>
    /// Load level tiếp theo
    /// </summary>
    public void NextLevel()
    {
        currentLevel++;
        
        if (LevelLoader.LevelExists(currentLevel))
        {
            if (levelManager != null)
            {
                levelManager.LoadLevel(currentLevel);
            }
            
            Debug.Log($"[GameController] Loading next level: {currentLevel}");
        }
        else
        {
            Debug.Log("[GameController] Đã hoàn thành tất cả levels!");
            OnGameComplete();
        }
    }
    
    /// <summary>
    /// Restart level hiện tại
    /// </summary>
    public void RestartLevel()
    {
        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
        
        Debug.Log($"[GameController] Restarting level {currentLevel}");
    }
    
    /// <summary>
    /// Load level cụ thể
    /// </summary>
    public void LoadSpecificLevel(int levelNumber)
    {
        if (LevelLoader.LevelExists(levelNumber))
        {
            currentLevel = levelNumber;
            
            if (levelManager != null)
            {
                levelManager.LoadLevel(levelNumber);
            }
            
            Debug.Log($"[GameController] Loading level {levelNumber}");
        }
        else
        {
            Debug.LogWarning($"[GameController] Level {levelNumber} không tồn tại!");
        }
    }
    
    /// <summary>
    /// Lưu tiến trình game
    /// </summary>
    private void SaveProgress()
    {
        // Lưu level cao nhất đã mở
        int highestLevel = PlayerPrefs.GetInt("HighestLevel", 1);
        if (currentLevel >= highestLevel)
        {
            PlayerPrefs.SetInt("HighestLevel", currentLevel + 1);
        }
        
        // Lưu level hiện tại
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[GameController] Progress saved - Next level: {currentLevel + 1}");
    }
    
    /// <summary>
    /// Reset tiến trình game
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.DeleteKey("HighestLevel");
        PlayerPrefs.Save();
        
        currentLevel = 1;
        LoadSpecificLevel(1);
        
        Debug.Log("[GameController] Progress reset to Level 1");
    }
    
    /// <summary>
    /// Khi hoàn thành tất cả levels
    /// </summary>
    private void OnGameComplete()
    {
        isGameActive = false;
        Debug.Log("[GameController] 🎉 Congratulations! All levels completed!");
    }
    
    /// <summary>
    /// Pause game
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("[GameController] Game paused");
    }
    
    /// <summary>
    /// Resume game
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameController] Game resumed");
    }
    
    /// <summary>
    /// Reload scene hiện tại
    /// </summary>
    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    /// <summary>
    /// Lấy level hiện tại
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// Lấy level cao nhất đã mở
    /// </summary>
    public int GetHighestLevel()
    {
        return PlayerPrefs.GetInt("HighestLevel", 1);
    }
}

