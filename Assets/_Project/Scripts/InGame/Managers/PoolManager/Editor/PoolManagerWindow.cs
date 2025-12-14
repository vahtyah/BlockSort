using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

public class PoolManagerWindow : EditorWindow
{
    private Vector2 _scrollPosition;
    private Dictionary<GameObject, bool> _poolFoldouts = new Dictionary<GameObject, bool>();
    private string _searchFilter = "";
    private bool _autoRefresh = true;
    private double _lastRefreshTime;
    private const float RefreshInterval = 0.5f;
    
    // Test Tools
    private bool _showTestTools = true;
    private GameObject _testPrefab;
    private int _testSpawnCount = 1;
    private List<GameObject> _testSpawnedObjects = new List<GameObject>();
    private Vector3 _testSpawnPosition = Vector3.zero;
    private bool _testSpawnRandomPosition = true;
    
    private enum SortMode
    {
        Name,
        TotalObjects,
        ActiveObjects,
        AvailableObjects
    }
    
    private SortMode _currentSortMode = SortMode.Name;
    private bool _sortDescending;
    
    [MenuItem("Window/Pool Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<PoolManagerWindow>("Pool Manager");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnEnable()
    {
        _lastRefreshTime = EditorApplication.timeSinceStartup;
    }
    
    private void Update()
    {
        if (Application.isPlaying && _autoRefresh)
        {
            if (EditorApplication.timeSinceStartup - _lastRefreshTime > RefreshInterval)
            {
                Repaint();
                _lastRefreshTime = EditorApplication.timeSinceStartup;
            }
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        DrawToolbar();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Pool Manager is only available during Play Mode.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }
        
        var manager = PoolManager.Instance;
        if (manager == null)
        {
            EditorGUILayout.HelpBox("PoolManager instance not found.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }
        
        DrawStatistics(manager);
        DrawTestTools(manager);
        DrawPoolList(manager);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        GUILayout.Label("Pool Manager", EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        
        _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton, GUILayout.Width(100));
        
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            Repaint();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawStatistics(PoolManager manager)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        var prefabToPoolField = typeof(PoolManager).GetField("prefabToPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var instanceToPoolField = typeof(PoolManager).GetField("instanceToPool", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (prefabToPoolField != null && instanceToPoolField != null)
        {
            var prefabToPool = prefabToPoolField.GetValue(manager) as System.Collections.IDictionary;
            var instanceToPool = instanceToPoolField.GetValue(manager) as System.Collections.IDictionary;
            
            if (prefabToPool != null && instanceToPool != null)
            {
                int totalPools = prefabToPool.Count;
                int totalActiveInstances = instanceToPool.Count;
                int totalAvailableObjects = 0;
                
                foreach (System.Collections.DictionaryEntry entry in prefabToPool)
                {
                    var pool = entry.Value;
                    var availableObjectsField = pool.GetType().GetField("availableObjects", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (availableObjectsField != null)
                    {
                        var availableObjects = availableObjectsField.GetValue(pool) as System.Collections.ICollection;
                        if (availableObjects != null)
                        {
                            totalAvailableObjects += availableObjects.Count;
                        }
                    }
                }
                
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                DrawStatBox("Pools", totalPools.ToString(), new Color(0.3f, 0.6f, 1f));
                DrawStatBox("Active", totalActiveInstances.ToString(), new Color(1f, 0.8f, 0.3f));
                DrawStatBox("Available", totalAvailableObjects.ToString(), new Color(0.5f, 1f, 0.5f));
                DrawStatBox("Total", (totalActiveInstances + totalAvailableObjects).ToString(), new Color(0.8f, 0.8f, 0.8f));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                if (totalPools > 0)
                {
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                    if (GUILayout.Button("Clear All Pools", GUILayout.Height(30)))
                    {
                        if (EditorUtility.DisplayDialog("Clear All Pools",
                            "Are you sure you want to clear all pools? This will destroy all pooled objects.",
                            "Yes", "Cancel"))
                        {
                            manager.ClearAllPools();
                        }
                    }
                    GUI.color = Color.white;
                }
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawStatBox(string label, string value, Color color)
    {
        GUI.color = color;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.color = Color.white;
        
        GUILayout.Label(label, EditorStyles.miniBoldLabel);
        GUILayout.Label(value, EditorStyles.largeLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTestTools(PoolManager manager)
    {
        EditorGUILayout.Space(10);
        
        _showTestTools = EditorGUILayout.Foldout(_showTestTools, "Test Tools", true, EditorStyles.foldoutHeader);
        
        if (_showTestTools)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Spawn Test", EditorStyles.boldLabel);
            
            // Test prefab selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Test Prefab:", GUILayout.Width(80));
            _testPrefab = EditorGUILayout.ObjectField(_testPrefab, typeof(GameObject), false) as GameObject;
            EditorGUILayout.EndHorizontal();
            
            // Spawn count
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spawn Count:", GUILayout.Width(80));
            _testSpawnCount = EditorGUILayout.IntSlider(_testSpawnCount, 1, 50);
            EditorGUILayout.EndHorizontal();
            
            // Position settings
            _testSpawnRandomPosition = EditorGUILayout.Toggle("Random Position", _testSpawnRandomPosition);
            
            if (!_testSpawnRandomPosition)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Position:", GUILayout.Width(80));
                _testSpawnPosition = EditorGUILayout.Vector3Field("", _testSpawnPosition);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(5);
            
            // Spawn buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = _testPrefab != null;
            GUI.color = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button($"Spawn {_testSpawnCount}x", GUILayout.Height(30)))
            {
                SpawnTestObjects();
            }
            GUI.color = Color.white;
            GUI.enabled = true;
            
            GUI.enabled = _testSpawnedObjects.Count > 0;
            GUI.color = new Color(1f, 0.8f, 0.3f);
            if (GUILayout.Button($"Despawn All ({_testSpawnedObjects.Count})", GUILayout.Height(30)))
            {
                DespawnAllTestObjects();
            }
            GUI.color = Color.white;
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Performance test
            EditorGUILayout.LabelField("Performance Test", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = _testPrefab != null;
            if (GUILayout.Button("Spawn 100x (Performance)", GUILayout.Height(25)))
            {
                PerformanceTestSpawn(100);
            }
            
            if (GUILayout.Button("Spawn 1000x (Stress)", GUILayout.Height(25)))
            {
                PerformanceTestSpawn(1000);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Test info
            if (_testSpawnedObjects.Count > 0)
            {
                EditorGUILayout.HelpBox($"Currently tracking {_testSpawnedObjects.Count} spawned test objects.", MessageType.Info);
            }
            else if (_testPrefab == null)
            {
                EditorGUILayout.HelpBox("Select a prefab to test spawning.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
    }
    
    private void SpawnTestObjects()
    {
        if (_testPrefab == null) return;
        
        var startTime = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < _testSpawnCount; i++)
        {
            Vector3 spawnPos = _testSpawnRandomPosition 
                ? new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(0f, 5f), UnityEngine.Random.Range(-10f, 10f))
                : _testSpawnPosition + new Vector3(i * 2f, 0, 0);
            
            var obj = Pool.Spawn(_testPrefab, spawnPos, Quaternion.identity);
            if (obj != null)
            {
                _testSpawnedObjects.Add(obj);
            }
        }
        
        startTime.Stop();
        Debug.Log($"[Pool Manager Test] Spawned {_testSpawnCount} objects in {startTime.ElapsedMilliseconds}ms");
    }
    
    private void DespawnAllTestObjects()
    {
        var startTime = System.Diagnostics.Stopwatch.StartNew();
        int count = _testSpawnedObjects.Count;
        
        foreach (var obj in _testSpawnedObjects)
        {
            if (obj != null)
            {
                Pool.Despawn(obj);
            }
        }
        
        _testSpawnedObjects.Clear();
        
        startTime.Stop();
        Debug.Log($"[Pool Manager Test] Despawned {count} objects in {startTime.ElapsedMilliseconds}ms");
    }
    
    private void PerformanceTestSpawn(int count)
    {
        if (_testPrefab == null) return;
        
        var startTime = System.Diagnostics.Stopwatch.StartNew();
        
        var tempList = new List<GameObject>();
        
        // Spawn
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(
                UnityEngine.Random.Range(-50f, 50f), 
                UnityEngine.Random.Range(0f, 10f), 
                UnityEngine.Random.Range(-50f, 50f)
            );
            
            var obj = Pool.Spawn(_testPrefab, spawnPos, Quaternion.identity);
            if (obj != null)
            {
                tempList.Add(obj);
            }
        }
        
        var spawnTime = startTime.ElapsedMilliseconds;
        startTime.Restart();
        
        // Despawn
        foreach (var obj in tempList)
        {
            if (obj != null)
            {
                Pool.Despawn(obj);
            }
        }
        
        var despawnTime = startTime.ElapsedMilliseconds;
        startTime.Stop();
        
        Debug.Log($"[Pool Manager Performance Test] {count} objects:\n" +
                  $"  Spawn: {spawnTime}ms ({(float)spawnTime / count:F2}ms per object)\n" +
                  $"  Despawn: {despawnTime}ms ({(float)despawnTime / count:F2}ms per object)\n" +
                  $"  Total: {spawnTime + despawnTime}ms");
    }
    
    private void DrawPoolList(PoolManager manager)
    {
        var prefabToPoolField = typeof(PoolManager).GetField("prefabToPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var instanceToPoolField = typeof(PoolManager).GetField("instanceToPool", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (prefabToPoolField == null || instanceToPoolField == null) return;
        
        var prefabToPool = prefabToPoolField.GetValue(manager) as System.Collections.IDictionary;
        var instanceToPool = instanceToPoolField.GetValue(manager) as System.Collections.IDictionary;
        
        if (prefabToPool == null || instanceToPool == null) return;
        
        if (prefabToPool.Count == 0)
        {
            EditorGUILayout.HelpBox("No pools created yet.", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space(10);
        
        // Search and Sort toolbar
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        _searchFilter = EditorGUILayout.TextField(_searchFilter);
        
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            _searchFilter = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sort by:", GUILayout.Width(50));
        _currentSortMode = (SortMode)EditorGUILayout.EnumPopup(_currentSortMode);
        _sortDescending = GUILayout.Toggle(_sortDescending, _sortDescending ? "↓" : "↑", GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Pool list
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        var sortedPools = GetSortedPools(prefabToPool, instanceToPool);
        
        foreach (var entry in sortedPools)
        {
            var prefab = entry.Key as GameObject;
            if (!string.IsNullOrEmpty(_searchFilter) && prefab != null)
            {
                if (!prefab.name.ToLower().Contains(_searchFilter.ToLower()))
                    continue;
            }
            
            DrawPoolInfoCompact(prefab, entry.Value, instanceToPool);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private List<System.Collections.DictionaryEntry> GetSortedPools(System.Collections.IDictionary prefabToPool, System.Collections.IDictionary instanceToPool)
    {
        var list = new List<System.Collections.DictionaryEntry>();
        foreach (System.Collections.DictionaryEntry entry in prefabToPool)
        {
            list.Add(entry);
        }
        
        switch (_currentSortMode)
        {
            case SortMode.Name:
                list.Sort((a, b) => 
                {
                    var aKey = a.Key as GameObject;
                    var bKey = b.Key as GameObject;
                    if (aKey == null || bKey == null) return 0;
                    return _sortDescending ? 
                        string.Compare(bKey.name, aKey.name, StringComparison.Ordinal) : 
                        string.Compare(aKey.name, bKey.name, StringComparison.Ordinal);
                });
                break;
                
            case SortMode.TotalObjects:
                list.Sort((a, b) => 
                {
                    int aTotal = GetTotalObjects(a.Value, instanceToPool);
                    int bTotal = GetTotalObjects(b.Value, instanceToPool);
                    return _sortDescending ? bTotal.CompareTo(aTotal) : aTotal.CompareTo(bTotal);
                });
                break;
                
            case SortMode.ActiveObjects:
                list.Sort((a, b) => 
                {
                    int aActive = GetActiveCount(a.Value, instanceToPool);
                    int bActive = GetActiveCount(b.Value, instanceToPool);
                    return _sortDescending ? bActive.CompareTo(aActive) : aActive.CompareTo(bActive);
                });
                break;
                
            case SortMode.AvailableObjects:
                list.Sort((a, b) => 
                {
                    int aAvailable = GetAvailableCount(a.Value);
                    int bAvailable = GetAvailableCount(b.Value);
                    return _sortDescending ? bAvailable.CompareTo(aAvailable) : aAvailable.CompareTo(bAvailable);
                });
                break;
        }
        
        return list;
    }
    
    private int GetTotalObjects(object pool, System.Collections.IDictionary instanceToPool)
    {
        return GetAvailableCount(pool) + GetActiveCount(pool, instanceToPool);
    }
    
    private int GetAvailableCount(object pool)
    {
        var availableObjectsField = pool.GetType().GetField("availableObjects", BindingFlags.NonPublic | BindingFlags.Instance);
        if (availableObjectsField != null)
        {
            var availableObjects = availableObjectsField.GetValue(pool) as System.Collections.ICollection;
            return availableObjects?.Count ?? 0;
        }
        return 0;
    }
    
    private int GetActiveCount(object pool, System.Collections.IDictionary instanceToPool)
    {
        int count = 0;
        foreach (System.Collections.DictionaryEntry entry in instanceToPool)
        {
            if (entry.Value == pool)
            {
                count++;
            }
        }
        return count;
    }
    
    private void DrawPoolInfoCompact(GameObject prefab, object pool, System.Collections.IDictionary instanceToPool)
    {
        if (prefab == null) return;
        
        var availableObjectsField = pool.GetType().GetField("availableObjects", BindingFlags.NonPublic | BindingFlags.Instance);
        var parentTransformField = pool.GetType().GetField("parentTransform", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (availableObjectsField == null || parentTransformField == null) return;
        
        var availableObjects = availableObjectsField.GetValue(pool) as Queue<GameObject>;
        var parentTransform = parentTransformField.GetValue(pool) as Transform;
        
        int availableCount = availableObjects?.Count ?? 0;
        int activeCount = GetActiveCount(pool, instanceToPool);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header
        EditorGUILayout.BeginHorizontal();
        
        if (!_poolFoldouts.ContainsKey(prefab))
        {
            _poolFoldouts[prefab] = false;
        }
        
        _poolFoldouts[prefab] = EditorGUILayout.Foldout(_poolFoldouts[prefab], "");
        
        EditorGUILayout.ObjectField(prefab, typeof(GameObject), false, GUILayout.Width(150));
        
        GUILayout.FlexibleSpace();
        
        // Stats with color coding
        GUI.color = new Color(0.5f, 1f, 0.5f);
        GUILayout.Label($"⚪ {availableCount}", GUILayout.Width(50));
        GUI.color = new Color(1f, 1f, 0.5f);
        GUILayout.Label($"⚫ {activeCount}", GUILayout.Width(50));
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.Label($"∑ {availableCount + activeCount}", GUILayout.Width(50));
        GUI.color = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Expanded view
        if (_poolFoldouts[prefab])
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parent:", GUILayout.Width(80));
            EditorGUILayout.ObjectField(parentTransform, typeof(Transform), true);
            EditorGUILayout.EndHorizontal();
            
            // Progress bar visualization
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Pool Usage:");
            DrawPoolUsageBar(activeCount, availableCount);
            
            EditorGUILayout.Space(5);
            
            // Clear pool button
            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Clear Pool", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear Pool",
                    $"Clear pool for '{prefab.name}'?",
                    "Yes", "Cancel"))
                {
                    var clearMethod = pool.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                    clearMethod?.Invoke(pool, null);
                }
            }
            GUI.color = Color.white;
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPoolUsageBar(int active, int available)
    {
        Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
        
        int total = active + available;
        if (total == 0) return;
        
        float activeRatio = (float)active / total;
        float availableRatio = (float)available / total;
        
        // Draw background
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        // Draw active portion
        Rect activeRect = new Rect(rect.x, rect.y, rect.width * activeRatio, rect.height);
        EditorGUI.DrawRect(activeRect, new Color(1f, 0.8f, 0.3f));
        
        // Draw available portion
        Rect availableRect = new Rect(rect.x + activeRect.width, rect.y, rect.width * availableRatio, rect.height);
        EditorGUI.DrawRect(availableRect, new Color(0.5f, 1f, 0.5f));
        
        // Draw label
        GUI.Label(rect, $"  Active: {active} | Available: {available}", EditorStyles.whiteLabel);
    }
}
