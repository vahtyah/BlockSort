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
    
    [Header("Scale Settings")]
    [SerializeField] private float desiredGap = 0.1f; // Khoảng cách mong muốn giữa các blocks
    [SerializeField] private float blockSize = 1f; // Kích thước thực của block prefab
    
    [Header("Grid Settings")]
    [SerializeField] private GridManager gridManager;
    
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
        
        // Tìm GridManager nếu chưa có
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogWarning("[LevelManager] GridManager không tìm thấy trong scene!");
            }
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
        
        // Auto-detect grid settings từ spawned blocks
        if (gridManager != null && spawnedBlocks.Count > 0)
        {
            gridManager.AutoDetectGridSettings(spawnedBlocks);
        }
        
        // Spawn boxes
        // SpawnBoxes();
        
        Debug.Log($"[LevelManager] Đã spawn {spawnedBlocks.Count} blocks và {spawnedBoxes.Count} boxes");
    }
    
    /// <summary>
    /// Spawn tất cả blocks với scale
    /// </summary>
    private void SpawnBlocks()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("[LevelManager] Block prefab chưa được gán!");
            return;
        }
        
        // Tính scale factor
        float scaleFactor = CalculateScaleFactor();
        
        foreach (BlockData blockData in currentLevelData.Blocks)
        {
            // Spawn nhiều blocks nếu StackSize > 1
            int stackSize = blockData.StackSize > 0 ? blockData.StackSize : 1;
            
            for (int stackIndex = 0; stackIndex < stackSize; stackIndex++)
            {
                GameObject blockObj = Instantiate(blockPrefab, levelContainer);
                
                // Tính toán vị trí cho mỗi block trong stack
                Vector3 stackOffset = Vector3.back * (stackIndex * scaleFactor * blockSize);
                blockObj.transform.position = blockData.Position + stackOffset;
                blockObj.transform.rotation = blockData.Rotation;
                blockObj.name = $"Block_{blockData.Index}_{stackIndex}";
                
                // Áp dụng scale
                blockObj.transform.localScale = Vector3.one * scaleFactor;
                
                // Set color
                Block block = blockObj.GetComponent<Block>();
                block.Initialize(blockData);
                if (block != null && blockData.Color >= 0 && blockData.Color < blockColors.Length)
                {
                    block.SetColor(blockColors[blockData.Color]);
                }
                
                spawnedBlocks.Add(blockObj);
                
                // Thêm block vào GridManager nếu có
                if (gridManager != null)
                {
                    gridManager.AddBlock(blockObj, blockObj.transform.position);
                }
            }
        }
        
        Debug.Log($"[LevelManager] Scale factor: {scaleFactor:F3}, Total spawned blocks: {spawnedBlocks.Count}");
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
        // Clear GridManager first
        if (gridManager != null)
        {
            gridManager.ClearGrid();
        }
        
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
    
    /// <summary>
    /// Tính scale factor dựa trên khoảng cách giữa các blocks
    /// </summary>
    private float CalculateScaleFactor()
    {
        float distanceInData = CalculateAverageDistance();
        
        if (distanceInData <= 0) return 1f;
        
        // Logic:
        // - Vị trí blocks KHÔNG thay đổi (giữ nguyên position từ data)
        // - Chỉ scale kích thước block (localScale)
        // - Khoảng cách trong data = distanceInData (khoảng cách giữa 2 tâm blocks)
        // - Kích thước block gốc = blockSize
        // - Sau khi scale với s: kích thước block mới = blockSize * s
        // - Gap thực tế = distanceInData - blockSize * s
        // - Ta muốn: gap thực tế = desiredGap
        // => distanceInData - blockSize * s = desiredGap
        // => blockSize * s = distanceInData - desiredGap
        // => s = (distanceInData - desiredGap) / blockSize
        
        float scaleFactor = (distanceInData - desiredGap) / blockSize;
        
        if (scaleFactor <= 0)
        {
            Debug.LogWarning($"[LevelManager] Scale factor ({scaleFactor}) <= 0. Distance: {distanceInData}, DesiredGap: {desiredGap}, BlockSize: {blockSize}");
            return 0.1f; // Scale tối thiểu
        }
        
        Debug.Log($"[LevelManager] Distance: {distanceInData:F3}, DesiredGap: {desiredGap:F3}, BlockSize: {blockSize:F3}, Scale: {scaleFactor:F3}");
        
        return scaleFactor;
    }
    
    /// <summary>
    /// Tính khoảng cách trung bình giữa các blocks trong level data
    /// Khoảng cách này bao gồm cả kích thước block
    /// </summary>
    private float CalculateAverageDistance()
    {
        if (currentLevelData == null || currentLevelData.Blocks.Count < 2)
            return 3.5f; // Giá trị mặc định
        
        // Tìm 2 blocks gần nhau nhất trong cùng một hàng (cùng Y và Z)
        float minDistance = float.MaxValue;
        bool foundPair = false;
        
        for (int i = 0; i < currentLevelData.Blocks.Count; i++)
        {
            for (int j = i + 1; j < currentLevelData.Blocks.Count; j++)
            {
                Vector3 pos1 = currentLevelData.Blocks[i].Position;
                Vector3 pos2 = currentLevelData.Blocks[j].Position;
                
                // Kiểm tra nếu 2 blocks cùng hàng (cùng Y và Z, chênh lệch nhỏ)
                if (Mathf.Abs(pos1.y - pos2.y) < 0.1f && Mathf.Abs(pos1.z - pos2.z) < 0.1f)
                {
                    float distance = Mathf.Abs(pos1.x - pos2.x);
                    if (distance > 0.01f && distance < minDistance)
                    {
                        minDistance = distance;
                        foundPair = true;
                    }
                }
            }
        }
        
        if (!foundPair)
        {
            // Nếu không tìm thấy cặp nào cùng hàng, lấy khoảng cách giữa 2 blocks đầu tiên
            Vector3 pos1 = currentLevelData.Blocks[0].Position;
            Vector3 pos2 = currentLevelData.Blocks[1].Position;
            minDistance = Vector3.Distance(pos1, pos2);
        }
        
        return minDistance;
    }
    
    private void OnDestroy()
    {
        ClearLevel();
    }
}

