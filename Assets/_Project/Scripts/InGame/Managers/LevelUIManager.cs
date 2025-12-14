using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Manager để kết nối UI với Level System
/// Attach vào GameObject UI Manager trong scene
/// </summary>
public class LevelUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private LevelManager levelManager;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI blocksCountText;
    [SerializeField] private TextMeshProUGUI boxesCountText;
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button pauseButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameplayPanel;
    
    private bool isPaused = false;
    
    private void Start()
    {
        // Auto-find nếu chưa gán
        if (gameController == null)
            gameController = FindObjectOfType<GameController>();
        
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
        
        // Setup button listeners
        SetupButtons();
        
        // Update UI lần đầu
        UpdateLevelInfo();
        
        // Hide panels
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }
    
    private void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClick);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClick);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonClick);
    }
    
    /// <summary>
    /// Update thông tin level trên UI
    /// </summary>
    public void UpdateLevelInfo()
    {
        if (levelManager == null) return;
        
        LevelData currentLevel = levelManager.GetCurrentLevelData();
        
        if (currentLevel != null)
        {
            // Update level number
            if (levelNumberText != null)
                levelNumberText.text = $"Level {currentLevel.LevelNumber}";
            
            // Update counts
            if (blocksCountText != null)
                blocksCountText.text = $"Blocks: {currentLevel.Blocks.Count}";
            
            if (boxesCountText != null)
                boxesCountText.text = $"Boxes: {currentLevel.Boxes.Count}";
        }
    }
    
    /// <summary>
    /// Hiển thị màn hình win
    /// </summary>
    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        
        if (gameplayPanel != null)
            gameplayPanel.SetActive(false);
    }
    
    /// <summary>
    /// Hide màn hình win
    /// </summary>
    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
        
        if (gameplayPanel != null)
            gameplayPanel.SetActive(true);
    }
    
    /// <summary>
    /// Restart button handler
    /// </summary>
    public void OnRestartButtonClick()
    {
        if (gameController != null)
        {
            gameController.RestartLevel();
            HideWinPanel();
            HidePausePanel();
            UpdateLevelInfo();
        }
    }
    
    /// <summary>
    /// Next button handler
    /// </summary>
    public void OnNextButtonClick()
    {
        if (gameController != null)
        {
            gameController.NextLevel();
            HideWinPanel();
            UpdateLevelInfo();
        }
    }
    
    /// <summary>
    /// Pause button handler
    /// </summary>
    public void OnPauseButtonClick()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Pause game
    /// </summary>
    public void PauseGame()
    {
        if (gameController != null)
            gameController.PauseGame();
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        isPaused = true;
    }
    
    /// <summary>
    /// Resume game
    /// </summary>
    public void ResumeGame()
    {
        if (gameController != null)
            gameController.ResumeGame();
        
        HidePausePanel();
        isPaused = false;
    }
    
    /// <summary>
    /// Hide pause panel
    /// </summary>
    public void HidePausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    
    /// <summary>
    /// Load level từ UI (dùng cho level selection)
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        if (gameController != null)
        {
            gameController.LoadSpecificLevel(levelNumber);
            UpdateLevelInfo();
        }
    }
    
    /// <summary>
    /// Gọi khi player hoàn thành level
    /// </summary>
    public void OnLevelComplete()
    {
        ShowWinPanel();
        
        if (gameController != null)
            gameController.CompleteLevel();
    }
}

