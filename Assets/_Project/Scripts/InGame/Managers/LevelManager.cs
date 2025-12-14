using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private GameObject boxPrefab;
    
    [Header("Level Settings")]
    [SerializeField] private int currentLevelNumber = 1;
    [SerializeField] private Transform levelContainer;
    
    [Header("Color Settings")]
    [SerializeField] private Color[] blockColors = new Color[]
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
    
    private LevelData currentLevelData;
    private List<GameObject> spawnedBlocks = new List<GameObject>();
    private List<GameObject> spawnedBoxes = new List<GameObject>();
    
    private void Start()
    {
        // Tạo container nếu chưa có
        if (levelContainer == null)
        {
            GameObject container = new GameObject("LevelContainer");
            levelContainer = container.transform;
            levelContainer.SetParent(transform);
        }
        
        // Load level đầu tiên
        LoadLevel(currentLevelNumber);
    }
    
    /// <summary>
    /// Load và spawn level
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        // Clear level hiện tại
        ClearLevel();
        
        // Load level data
        currentLevelData = LevelLoader.LoadLevel(levelNumber);
        
        if (currentLevelData == null)
        {
            Debug.LogError($"[LevelManager] Không thể load level {levelNumber}");
            return;
        }
        
        currentLevelNumber = levelNumber;
        
        // Spawn level
        SpawnLevel();
        
        Debug.Log($"[LevelManager] Đã load Level {levelNumber}");
    }
    
    /// <summary>
    /// Spawn tất cả blocks và boxes trong level
    /// </summary>
    private void SpawnLevel()
    {
        if (currentLevelData == null) return;
        
        // Spawn blocks
        SpawnBlocks();
        
        // Spawn boxes
        // SpawnBoxes();
        
        Debug.Log($"[LevelManager] Đã spawn {spawnedBlocks.Count} blocks và {spawnedBoxes.Count} boxes");
    }
    
    /// <summary>
    /// Spawn tất cả blocks
    /// </summary>
    private void SpawnBlocks()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("[LevelManager] Block prefab chưa được gán!");
            return;
        }
        
        foreach (BlockData blockData in currentLevelData.Blocks)
        {
            GameObject blockObj = Instantiate(blockPrefab, levelContainer);
            blockObj.transform.position = blockData.Position;
            blockObj.transform.rotation = blockData.Rotation;
            blockObj.name = $"Block_{blockData.Index}";
            
            // Set color
            Block block = blockObj.GetComponent<Block>();
            if (block != null && blockData.Color >= 0 && blockData.Color < blockColors.Length)
            {
                block.SetColor(blockColors[blockData.Color]);
            }
            
            spawnedBlocks.Add(blockObj);
        }
    }
    
    /// <summary>
    /// Spawn tất cả boxes
    /// </summary>
    private void SpawnBoxes()
    {
        if (boxPrefab == null)
        {
            Debug.LogWarning("[LevelManager] Box prefab chưa được gán!");
            return;
        }
        
        foreach (BoxData boxData in currentLevelData.Boxes)
        {
            GameObject boxObj = Instantiate(boxPrefab, levelContainer);
            boxObj.transform.position = boxData.Position;
            boxObj.transform.rotation = boxData.Rotation;
            boxObj.name = $"Box_Type{boxData.Type}";
            
            // Set color cho box (nếu box có component tương tự Block)
            Block boxBlock = boxObj.GetComponent<Block>();
            if (boxBlock != null && boxData.Color >= 0 && boxData.Color < blockColors.Length)
            {
                boxBlock.SetColor(blockColors[boxData.Color]);
            }
            
            spawnedBoxes.Add(boxObj);
        }
    }
    
    /// <summary>
    /// Xóa tất cả objects trong level hiện tại
    /// </summary>
    public void ClearLevel()
    {
        // Xóa blocks
        foreach (GameObject block in spawnedBlocks)
        {
            if (block != null)
                Destroy(block);
        }
        spawnedBlocks.Clear();
        
        // Xóa boxes
        foreach (GameObject box in spawnedBoxes)
        {
            if (box != null)
                Destroy(box);
        }
        spawnedBoxes.Clear();
    }
    
    /// <summary>
    /// Load level tiếp theo
    /// </summary>
    public void LoadNextLevel()
    {
        int nextLevel = currentLevelNumber + 1;
        
        if (LevelLoader.LevelExists(nextLevel))
        {
            LoadLevel(nextLevel);
        }
        else
        {
            Debug.Log($"[LevelManager] Không có level {nextLevel}. Đây là level cuối cùng!");
        }
    }
    
    /// <summary>
    /// Load lại level hiện tại
    /// </summary>
    public void RestartLevel()
    {
        LoadLevel(currentLevelNumber);
    }
    
    /// <summary>
    /// Lấy thông tin level hiện tại
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }
    
    /// <summary>
    /// Lấy số level hiện tại
    /// </summary>
    public int GetCurrentLevelNumber()
    {
        return currentLevelNumber;
    }
    
    private void OnDestroy()
    {
        ClearLevel();
    }
}

