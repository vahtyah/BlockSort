using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script hỗ trợ setup các component cần thiết cho Grid System
/// Attach vào một GameObject trong scene và chạy setup từ Inspector
/// </summary>
public class GridSystemSetupHelper : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool autoSetupOnStart = false;
    
    [Header("Current References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Canvas canvas;
    
    [Header("UI Settings")]
    [SerializeField] private Vector2 buttonContainerPosition = new Vector2(0, -300);
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGridSystem();
        }
    }
    
    [ContextMenu("Setup Grid System")]
    public void SetupGridSystem()
    {
        Debug.Log("[GridSystemSetupHelper] Bắt đầu setup Grid System...");
        
        // 1. Find or create GridManager
        SetupGridManager();
        
        // 2. Setup LevelManager reference
        SetupLevelManager();
        
        // 3. Setup UI
        SetupUI();
        
        // 4. Setup ColorButtonController
        
        Debug.Log("[GridSystemSetupHelper] ✓ Hoàn thành setup Grid System!");
        LogSetupInstructions();
    }
    
    private void SetupGridManager()
    {
        gridManager = FindObjectOfType<GridManager>();
        
        if (gridManager == null)
        {
            GameObject gridManagerObj = new GameObject("GridManager");
            gridManager = gridManagerObj.AddComponent<GridManager>();
            Debug.Log("[GridSystemSetupHelper] ✓ Đã tạo GridManager");
        }
        else
        {
            Debug.Log("[GridSystemSetupHelper] ✓ Tìm thấy GridManager");
        }
    }
    
    private void SetupLevelManager()
    {
        levelManager = FindObjectOfType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogWarning("[GridSystemSetupHelper] ⚠ Không tìm thấy LevelManager trong scene!");
            Debug.LogWarning("[GridSystemSetupHelper] → Vui lòng thêm LevelManager vào scene và gán GridManager vào nó.");
        }
        else
        {
            Debug.Log("[GridSystemSetupHelper] ✓ Tìm thấy LevelManager");
            Debug.Log("[GridSystemSetupHelper] → Hãy đảm bảo đã gán GridManager vào LevelManager trong Inspector");
        }
    }
    
    private void SetupUI()
    {
        canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            // Tạo Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[GridSystemSetupHelper] ✓ Đã tạo Canvas");
        }
        else
        {
            Debug.Log("[GridSystemSetupHelper] ✓ Tìm thấy Canvas");
        }
        
        // Tìm hoặc tạo Button Container
        Transform buttonContainer = canvas.transform.Find("ColorButtonContainer");
        
        if (buttonContainer == null)
        {
            GameObject containerObj = new GameObject("ColorButtonContainer");
            containerObj.transform.SetParent(canvas.transform);
            
            RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = buttonContainerPosition;
            rectTransform.sizeDelta = new Vector2(800, 100);
            
            HorizontalLayoutGroup layoutGroup = containerObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            
            buttonContainer = containerObj.transform;
            Debug.Log("[GridSystemSetupHelper] ✓ Đã tạo ColorButtonContainer");
        }
        else
        {
            Debug.Log("[GridSystemSetupHelper] ✓ Tìm thấy ColorButtonContainer");
        }
    }
    
    private void LogSetupInstructions()
    {
        Debug.Log("========================================");
        Debug.Log("CÁC BƯỚC TIẾP THEO:");
        Debug.Log("1. Trong LevelManager Inspector:");
        Debug.Log("   → Gán GridManager vào field 'Grid Manager'");
        Debug.Log("");
        Debug.Log("2. Trong ColorButtonController Inspector:");
        Debug.Log("   → Gán GridManager vào field 'Grid Manager'");
        Debug.Log("   → Gán ColorButtonContainer vào field 'Button Container'");
        Debug.Log("");
        Debug.Log("3. Kiểm tra Available Colors trong ColorButtonController");
        Debug.Log("   → Đảm bảo khớp với Block Colors trong LevelManager");
        Debug.Log("");
        Debug.Log("4. Chạy game và test!");
        Debug.Log("========================================");
    }
    
    [ContextMenu("Verify Setup")]
    public void VerifySetup()
    {
        Debug.Log("[GridSystemSetupHelper] Kiểm tra setup...");
        
        bool isValid = true;
        
        // Check GridManager
        if (FindObjectOfType<GridManager>() == null)
        {
            Debug.LogError("✗ Không tìm thấy GridManager!");
            isValid = false;
        }
        else
        {
            Debug.Log("✓ GridManager OK");
        }
        
        // Check LevelManager
        LevelManager lm = FindObjectOfType<LevelManager>();
        if (lm == null)
        {
            Debug.LogError("✗ Không tìm thấy LevelManager!");
            isValid = false;
        }
        else
        {
            Debug.Log("✓ LevelManager OK");
        }
        
        // Check ColorButtonController
        
        // Check Canvas and ButtonContainer
        Canvas c = FindObjectOfType<Canvas>();
        if (c == null)
        {
            Debug.LogError("✗ Không tìm thấy Canvas!");
            isValid = false;
        }
        else
        {
            Transform bc = c.transform.Find("ColorButtonContainer");
            if (bc == null)
            {
                Debug.LogError("✗ Không tìm thấy ColorButtonContainer trong Canvas!");
                isValid = false;
            }
            else
            {
                Debug.Log("✓ Canvas và ColorButtonContainer OK");
            }
        }
        
        if (isValid)
        {
            Debug.Log("========================================");
            Debug.Log("✓✓✓ TẤT CẢ COMPONENTS ĐÃ SẴN SÀNG! ✓✓✓");
            Debug.Log("Hãy đảm bảo đã gán các references trong Inspector");
            Debug.Log("========================================");
        }
        else
        {
            Debug.LogError("========================================");
            Debug.LogError("⚠ CÒN VẤN ĐỀ CẦN KHẮC PHỤC!");
            Debug.LogError("Chạy 'Setup Grid System' để tự động tạo components thiếu");
            Debug.LogError("========================================");
        }
    }
}

