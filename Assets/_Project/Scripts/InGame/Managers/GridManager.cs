using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 10;
    [SerializeField] private int columns = 8;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero; // Điểm gốc của grid
    [SerializeField] private float cellSpacing = 1f; // Khoảng cách giữa các ô
    
    [Header("Animation Settings")]
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float removalDelay = 0.2f;
    
    [Header("Removal Settings")]
    [SerializeField] private int maxBlocksToRemove = 3; // Số lượng blocks tối đa bị xóa mỗi lần
    [SerializeField] private bool removeFromBottomOnly = true; // Chỉ xóa từ hàng cuối
    
    [Header("Events")]
    public UnityEvent<int> OnBlocksRemoved; // Số lượng blocks bị xóa
    
    // Grid 2D lưu trữ blocks [column][row] - column từ trái qua phải, row từ dưới lên
    private GridCell[,] grid;
    
    // Đang xử lý animation
    private bool isProcessing = false;
    
    private void Awake()
    {
        InitializeGrid();
    }
    
    /// <summary>
    /// Khởi tạo grid rỗng
    /// </summary>
    private void InitializeGrid()
    {
        grid = new GridCell[columns, rows];
        
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                grid[col, row] = new GridCell();
            }
        }
        
        Debug.Log($"[GridManager] Đã khởi tạo grid {columns}x{rows}");
    }
    
    /// <summary>
    /// Thêm block vào grid dựa trên vị trí world position
    /// </summary>
    public bool AddBlock(GameObject blockObj, Vector3 worldPosition)
    {
        // Chuyển world position thành grid coordinates
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        
        if (!IsValidGridPosition(gridPos))
        {
            Debug.LogWarning($"[GridManager] Vị trí grid không hợp lệ: {gridPos} từ world pos: {worldPosition}");
            return false;
        }
        
        // Lưu block vào grid
        Block block = blockObj.GetComponent<Block>();
        if (block == null)
        {
            Debug.LogError("[GridManager] GameObject không có component Block!");
            return false;
        }
        
        // Kiểm tra nếu ô đã có block
        if (!grid[gridPos.x, gridPos.y].isEmpty)
        {
            Debug.LogWarning($"[GridManager] Ô [{gridPos.x}, {gridPos.y}] đã có block!");
        }
        
        grid[gridPos.x, gridPos.y].blockObject = blockObj;
        grid[gridPos.x, gridPos.y].block = block;
        grid[gridPos.x, gridPos.y].isEmpty = false;
        
        return true;
    }
    
    /// <summary>
    /// Xóa tất cả blocks có màu chỉ định
    /// </summary>
    public void RemoveBlocksByColor(int index)
    {
        if (isProcessing)
        {
            Debug.LogWarning("[GridManager] Đang xử lý animation, vui lòng đợi!");
            return;
        }
        
        StartCoroutine(RemoveBlocksCoroutine(index));
    }
    
    /// <summary>
    /// Coroutine xóa blocks và áp dụng gravity
    /// </summary>
    private IEnumerator RemoveBlocksCoroutine(int targetColor)
    {
        isProcessing = true;
        
        Debug.Log($"[GridManager] 🎯 Tìm blocks màu {targetColor} để xóa...");
        Debug.Log($"[GridManager] Settings: RemoveFromBottomOnly={removeFromBottomOnly}, MaxBlocks={maxBlocksToRemove}");
        
        // Tìm blocks có màu matching
        List<Vector2Int> blocksToRemove = new List<Vector2Int>();
        
        if (removeFromBottomOnly)
        {
            // Chỉ tìm ở hàng cuối cùng (row 0 - bottom row)
            Debug.Log("[GridManager] 🔍 Tìm kiếm ở HÀNG CUỐI CÙNG (row 0)...");
            
            for (int col = 0; col < columns; col++)
            {
                // Tìm block thấp nhất trong cột này
                int bottomRow = FindLowestNonEmptyRow(col);
                
                if (bottomRow >= 0 && !grid[col, bottomRow].isEmpty && grid[col, bottomRow].block != null)
                {
                    var blockColor = grid[col, bottomRow].block.GetColor();
                    Debug.Log($"[GridManager] Col {col}, Row {bottomRow}: Block màu {blockColor})");
                    
                    if (blockColor == targetColor)
                    {
                        blocksToRemove.Add(new Vector2Int(col, bottomRow));
                        Debug.Log($"[GridManager] ✓ MATCH! Thêm vào danh sách xóa");
                        
                        // Giới hạn số lượng
                        if (blocksToRemove.Count >= maxBlocksToRemove)
                        {
                            Debug.Log($"[GridManager] Đã đạt giới hạn {maxBlocksToRemove} blocks");
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            // Tìm tất cả blocks có màu matching (logic cũ)
            Debug.Log("[GridManager] 🔍 Tìm kiếm trong TẤT CẢ grid...");
            
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (!grid[col, row].isEmpty && grid[col, row].block != null)
                    {
                        if ((grid[col, row].block.GetColor() == targetColor))
                        {
                            blocksToRemove.Add(new Vector2Int(col, row));
                            
                            // Giới hạn số lượng
                            if (maxBlocksToRemove > 0 && blocksToRemove.Count >= maxBlocksToRemove)
                            {
                                Debug.Log($"[GridManager] Đã đạt giới hạn {maxBlocksToRemove} blocks");
                                break;
                            }
                        }
                    }
                }
                
                if (maxBlocksToRemove > 0 && blocksToRemove.Count >= maxBlocksToRemove)
                    break;
            }
        }
        
        if (blocksToRemove.Count == 0)
        {
            Debug.LogWarning("[GridManager] ⚠️ Không tìm thấy blocks có màu phù hợp ở hàng cuối!");
            isProcessing = false;
            yield break;
        }
        
        Debug.Log($"[GridManager] ✅ Tìm thấy {blocksToRemove.Count} blocks để xóa");
        
        // Xóa blocks
        foreach (Vector2Int pos in blocksToRemove)
        {
            Debug.Log($"[GridManager] 🗑️ Xóa block tại [{pos.x}, {pos.y}]");
            if (grid[pos.x, pos.y].blockObject != null)
            {
                Destroy(grid[pos.x, pos.y].blockObject);
            }
            grid[pos.x, pos.y].Clear();
        }
        
        OnBlocksRemoved?.Invoke(blocksToRemove.Count);
        
        // Đợi một chút trước khi áp dụng gravity
        yield return new WaitForSeconds(removalDelay);
        
        // Áp dụng gravity
        Debug.Log("[GridManager] ⬇️ Áp dụng gravity...");
        yield return StartCoroutine(ApplyGravity());
        
        isProcessing = false;
        Debug.Log("[GridManager] ✓ Hoàn thành!");
    }
    
    /// <summary>
    /// Áp dụng gravity - các blocks rơi xuống vị trí trống
    /// </summary>
    private IEnumerator ApplyGravity()
    {
        bool anyBlockMoved = true;
        
        while (anyBlockMoved)
        {
            anyBlockMoved = false;
            List<BlockMove> moves = new List<BlockMove>();
            
            // Duyệt từ dưới lên, từ trái qua phải
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows - 1; row++)
                {
                    // Nếu ô hiện tại trống và ô phía trên có block
                    if (grid[col, row].isEmpty && !grid[col, row + 1].isEmpty)
                    {
                        // Di chuyển block xuống
                        moves.Add(new BlockMove
                        {
                            fromCol = col,
                            fromRow = row + 1,
                            toCol = col,
                            toRow = row
                        });
                        anyBlockMoved = true;
                    }
                }
            }
            
            // Thực hiện di chuyển
            foreach (BlockMove move in moves)
            {
                // Cập nhật grid
                grid[move.toCol, move.toRow] = grid[move.fromCol, move.fromRow];
                grid[move.fromCol, move.fromRow].Clear();
                
                // Animate block falling
                if (grid[move.toCol, move.toRow].blockObject != null)
                {
                    Vector3 targetPos = GridToWorldPosition(new Vector2Int(move.toCol, move.toRow));
                    StartCoroutine(AnimateBlockFall(grid[move.toCol, move.toRow].blockObject, targetPos));
                }
            }
            
            // Đợi animation hoàn thành
            if (moves.Count > 0)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
    
    /// <summary>
    /// Animate block rơi xuống
    /// </summary>
    private IEnumerator AnimateBlockFall(GameObject blockObj, Vector3 targetPosition)
    {
        if (blockObj == null) yield break;
        
        Vector3 startPos = blockObj.transform.position;
        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, targetPosition) / fallSpeed;
        
        while (elapsed < duration)
        {
            if (blockObj == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            blockObj.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
        
        if (blockObj != null)
        {
            blockObj.transform.position = targetPosition;
        }
    }
    
    /// <summary>
    /// Chuyển world position thành grid position
    /// </summary>
    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        // Tính offset từ grid origin
        Vector3 offset = worldPos - gridOrigin;
        
        // Chuyển đổi sang grid coordinates
        // X axis -> columns, Y axis -> rows
        int col = Mathf.RoundToInt(offset.x / cellSpacing);
        int row = Mathf.RoundToInt(offset.y / cellSpacing);
        
        return new Vector2Int(col, row);
    }
    
    /// <summary>
    /// Chuyển grid position thành world position
    /// </summary>
    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        Vector3 worldPos = gridOrigin;
        worldPos.x += gridPos.x * cellSpacing;
        worldPos.y += gridPos.y * cellSpacing;
        return worldPos;
    }
    
    /// <summary>
    /// Kiểm tra vị trí grid có hợp lệ không
    /// </summary>
    private bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < columns && 
               gridPos.y >= 0 && gridPos.y < rows;
    }
    
    /// <summary>
    /// Tìm hàng trống thấp nhất trong cột
    /// </summary>
    private int FindLowestEmptyRow(int column)
    {
        for (int row = 0; row < rows; row++)
        {
            if (grid[column, row].isEmpty)
            {
                return row;
            }
        }
        return -1; // Cột đầy
    }
    
    /// <summary>
    /// Tìm hàng KHÔNG TRỐNG thấp nhất trong cột (block ở đáy)
    /// </summary>
    private int FindLowestNonEmptyRow(int column)
    {
        for (int row = 0; row < rows; row++)
        {
            if (!grid[column, row].isEmpty)
            {
                return row;
            }
        }
        return -1; // Cột rỗng
    }
    
    /// <summary>
    /// Auto-detect grid settings từ spawned blocks
    /// </summary>
    public void AutoDetectGridSettings(List<GameObject> blocks)
    {
        if (blocks == null || blocks.Count < 2)
        {
            Debug.LogWarning("[GridManager] Cần ít nhất 2 blocks để tự động phát hiện grid");
            return;
        }
        
        // Tìm min/max positions
        Vector3 minPos = blocks[0].transform.position;
        Vector3 maxPos = blocks[0].transform.position;
        
        foreach (GameObject block in blocks)
        {
            if (block == null) continue;
            
            Vector3 pos = block.transform.position;
            minPos = Vector3.Min(minPos, pos);
            maxPos = Vector3.Max(maxPos, pos);
        }
        
        // Set grid origin
        gridOrigin = minPos;
        
        // Tìm khoảng cách nhỏ nhất giữa các blocks để xác định cellSpacing
        float minDistance = float.MaxValue;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] == null) continue;
            
            for (int j = i + 1; j < blocks.Count; j++)
            {
                if (blocks[j] == null) continue;
                
                float dist = Vector3.Distance(blocks[i].transform.position, blocks[j].transform.position);
                if (dist > 0.01f && dist < minDistance)
                {
                    minDistance = dist;
                }
            }
        }
        
        if (minDistance < float.MaxValue)
        {
            cellSpacing = minDistance;
        }
        
        // Tính số columns và rows
        columns = Mathf.RoundToInt((maxPos.x - minPos.x) / cellSpacing) + 1;
        rows = Mathf.RoundToInt((maxPos.y - minPos.y) / cellSpacing) + 1;
        
        // Re-initialize grid với kích thước mới
        InitializeGrid();
        
        Debug.Log($"[GridManager] Auto-detected: Origin={gridOrigin}, Spacing={cellSpacing:F2}, Grid={columns}x{rows}");
    }
    
    /// <summary>
    /// Xóa toàn bộ grid
    /// </summary>
    public void ClearGrid()
    {
        if (grid != null)
        {
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    grid[col, row].Clear();
                }
            }
        }
        
        Debug.Log("[GridManager] Đã xóa grid");
    }
    
    /// <summary>
    /// Lấy số lượng blocks còn lại trong grid
    /// </summary>
    public int GetBlockCount()
    {
        int count = 0;
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (!grid[col, row].isEmpty)
                    count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Debug: In grid ra console
    /// </summary>
    public void PrintGrid()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[GridManager] Grid Status:");
        
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < columns; col++)
            {
                sb.Append(grid[col, row].isEmpty ? "[ ]" : "[X]");
            }
            sb.AppendLine();
        }
        
        Debug.Log(sb.ToString());
    }
}

/// <summary>
/// Lưu trữ thông tin của một ô trong grid
/// </summary>
[System.Serializable]
public class GridCell
{
    public GameObject blockObject;
    public Block block;
    public bool isEmpty = true;
    
    public void Clear()
    {
        blockObject = null;
        block = null;
        isEmpty = true;
    }
}

/// <summary>
/// Thông tin di chuyển block
/// </summary>
public struct BlockMove
{
    public int fromCol;
    public int fromRow;
    public int toCol;
    public int toRow;
}

