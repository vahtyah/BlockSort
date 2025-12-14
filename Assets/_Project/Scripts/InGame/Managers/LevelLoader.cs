using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelLoader
{
    // Đường dẫn Resources (phải tạo folder: Assets/_Project/Resources/Levels/)
    private const string RESOURCES_PATH = "_Project/Levels";
    
    // Đường dẫn Assets (cho Editor)
    private const string ASSETS_PATH = "Assets/_Project/Levels";
    
    /// <summary>
    /// Load level data từ Assets/_Project/Levels
    /// </summary>
    /// <param name="levelNumber">Số level cần load</param>
    /// <returns>LevelData hoặc null nếu không tìm thấy</returns>
    public static LevelData LoadLevel(int levelNumber)
    {
#if UNITY_EDITOR
        // Trong Editor, load trực tiếp từ Assets
        string assetPath = $"{ASSETS_PATH}/Level_{levelNumber}.asset";
        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
        
        if (levelData == null)
        {
            Debug.LogError($"[LevelLoader] Không tìm thấy level {levelNumber} tại: {assetPath}");
            return null;
        }
        
        Debug.Log($"[LevelLoader] Đã load Level {levelNumber} từ {assetPath} - Blocks: {levelData.Blocks.Count}, Boxes: {levelData.Boxes.Count}");
        return levelData;
#else
        // Trong Build, load từ Resources
        string resourcePath = $"{RESOURCES_PATH}/Level_{levelNumber}";
        LevelData levelData = Resources.Load<LevelData>(resourcePath);
        
        if (levelData == null)
        {
            Debug.LogError($"[LevelLoader] Không tìm thấy level {levelNumber} tại: Resources/{resourcePath}");
            return null;
        }
        
        Debug.Log($"[LevelLoader] Đã load Level {levelNumber} - Blocks: {levelData.Blocks.Count}, Boxes: {levelData.Boxes.Count}");
        return levelData;
#endif
    }
    
    /// <summary>
    /// Load tất cả levels từ Assets/_Project/Levels
    /// </summary>
    /// <returns>Mảng các LevelData</returns>
    public static LevelData[] LoadAllLevels()
    {
#if UNITY_EDITOR
        // Trong Editor, tìm tất cả assets
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { ASSETS_PATH });
        LevelData[] levels = new LevelData[guids.Length];
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            levels[i] = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        }
        
        if (levels.Length == 0)
        {
            Debug.LogWarning($"[LevelLoader] Không tìm thấy level nào trong {ASSETS_PATH}");
        }
        else
        {
            Debug.Log($"[LevelLoader] Đã load {levels.Length} levels từ {ASSETS_PATH}");
        }
        
        return levels;
#else
        // Trong Build, load từ Resources
        LevelData[] levels = Resources.LoadAll<LevelData>(RESOURCES_PATH);
        
        if (levels.Length == 0)
        {
            Debug.LogWarning($"[LevelLoader] Không tìm thấy level nào trong Resources/{RESOURCES_PATH}");
        }
        else
        {
            Debug.Log($"[LevelLoader] Đã load {levels.Length} levels");
        }
        
        return levels;
#endif
    }
    
    /// <summary>
    /// Kiểm tra level có tồn tại không
    /// </summary>
    public static bool LevelExists(int levelNumber)
    {
#if UNITY_EDITOR
        string assetPath = $"{ASSETS_PATH}/Level_{levelNumber}.asset";
        return AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null;
#else
        string resourcePath = $"{RESOURCES_PATH}/Level_{levelNumber}";
        return Resources.Load<LevelData>(resourcePath) != null;
#endif
    }
}

